using UnityEngine;
using Proto4z;
using System.Collections;
using UnityEngine.UI;
using System;

public class ServerProxy : MonoBehaviour
{

    Session _client;
	Session _sceneSession;
    public SessionStatus ClientStatus { get { return _client == null ?  SessionStatus.SS_UNINIT: _client.Status; } }
    string _account;
    string _passwd;
    GameObject _busyTips;
    GameObject _chatPanel;
	Transform _selectScene = null;
	Transform _scene = null;
    void Awake()
    {
        Debug.Log("Awake ServerProxy.");
        DontDestroyOnLoad(gameObject);
        _busyTips = GameObject.Find("BusyTips");
        _chatPanel = GameObject.Find("ChatUI");
        Facade.GetSingleton<Dispatcher>().AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade.GetSingleton<Dispatcher>().AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AvatarBaseInfoNotice", (System.Action<AvatarBaseInfoNotice>)OnAvatarBaseInfoNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);

		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupInfoNotice", (System.Action<SceneGroupInfoNotice>)OnSceneGroupInfoNotice);
		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupGetResp", (System.Action<SceneGroupGetResp>)OnSceneGroupGetResp);
		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupEnterResp", (System.Action<SceneGroupEnterResp>)OnSceneGroupEnterResp);
		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupCancelResp", (System.Action<SceneGroupCancelResp>)OnSceneGroupCancelResp);

		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupCreateResp", (System.Action<SceneGroupCreateResp>)OnSceneGroupCreateResp);
		Facade.GetSingleton<Dispatcher>().AddListener("SceneGroupLeaveResp", (System.Action<SceneGroupLeaveResp>)OnSceneGroupLeaveResp);
    
        Facade.GetSingleton<Dispatcher>().AddListener("SceneSectionNotice", (System.Action<SceneSectionNotice>)OnSceneSectionNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("SceneRefreshNotice", (System.Action<SceneRefreshNotice>)OnSceneRefreshNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("AddEntityNotice", (System.Action<AddEntityNotice>)OnAddEntityNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("RemoveEntityNotice", (System.Action<RemoveEntityNotice>)OnRemoveEntityNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("MoveNotice", (System.Action<MoveNotice>)OnMoveNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("AddBuffNotice", (System.Action<AddBuffNotice>)OnAddBuffNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("RemoveBuffNotice", (System.Action<RemoveBuffNotice>)OnRemoveBuffNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("UseSkillNotice", (System.Action<UseSkillNotice>)OnUseSkillNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("MoveResp", (System.Action<MoveResp>)OnMoveResp);
        Facade.GetSingleton<Dispatcher>().AddListener("UseSkillResp", (System.Action<UseSkillResp>)OnUseSkillResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AttachSceneResp", (System.Action<AttachSceneResp>)OnAttachSceneResp);

    }
    void Start()
    {
        Debug.logger.Log("ServerProxy::Start ");
    }

    public void Login(string host, ushort port, string account, string pwd)
    {
        _account = account;
        _passwd = pwd;
        if (_client != null)
        {
            _client.Close();
        }
        _client = new Session();
        _client._onConnect = (Action)OnConnect;
        _client.Init(host, port, "");
        
    }
    public void OnConnect()
    {
        _client.Send(new ClientAuthReq(_account, _passwd));
        if (_chatPanel !=null && !_chatPanel.activeSelf)
        {
            _chatPanel.SetActive(true);
        }
    }

    public void Send<T>(T proto) where T : Proto4z.IProtoObject
    {
        _client.Send(proto);
    }
    void OnClientAuthResp(ClientAuthResp resp)
    {
        var account = resp.account;
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS)
        {
            Debug.logger.Log(LogType.Error, "ServerProxy::OnClientAuthResp account=" + account);
            return;
        }
        Debug.logger.Log("ServerProxy::OnClientAuthResp account=" + account);
        if (resp.previews.Count == 0)
        {
            _client.Send(new CreateAvatarReq("", _account));
        }
        else
        {
            _client.Send(new AttachAvatarReq("", resp.previews[0].avatarID));
        }
    }

    void OnCreateAvatarResp(CreateAvatarResp resp)
    {
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS || resp.previews.Count == 0)
        {
            Debug.logger.Log(LogType.Error, "NetController::OnCreateAvatarResp ");
            return;
        }
        Debug.logger.Log("NetController::OnCreateAvatarResp ");
        _client.Send(new AttachAvatarReq("", resp.previews[0].avatarID));
    }

    void OnAttachAvatarResp(AttachAvatarResp resp)
    {
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS )
        {
            Debug.LogError("NetController::AttachAvatarResp ");
            return;
        }
        if (Facade.AvatarInfo != null)
        {
            Debug.LogError("NetController::AttachAvatarResp alread had attach");
            return;
        }
		if (_selectScene == null)
		{
			var sceneUI = Resources.Load<GameObject>("Guis/SelectScene/SelectScene");
			if (sceneUI != null)
			{
				_selectScene = Instantiate(sceneUI).transform;
				_selectScene.SetParent(GameObject.Find("MainUI").transform, false);
				_selectScene.gameObject.SetActive(true);
				_selectScene.Find("ExitScene").GetComponent<Button>().onClick.AddListener(delegate () { OnExitScene(); });
				_selectScene.Find("HomeScene").GetComponent<Button>().onClick.AddListener(delegate () { OnHomeScene(); });
				_selectScene.Find("ArenaScene").GetComponent<Button>().onClick.AddListener(delegate () { OnArenaScene(); });
			}
			else
			{
				Debug.LogError("can't Instantiate [Prefabs/Guis/SelectScene/SelectScene].");
			}
		}
        Facade.AvatarInfo = resp.baseInfo;
		_client.Send (new SceneGroupGetReq ());

        Debug.logger.Log("NetController::AttachAvatarResp ");
        PingPongSend();

        var login = GameObject.Find("LoginUI");
        if (login != null)
        {
            login.SetActive(false);
        }
    }
    void CreateSceneSession(ulong avatarID,  Proto4z.SceneGroupInfo groupInfo)
    {
        _sceneSession = new Session();
        _sceneSession.Init(groupInfo.host, groupInfo.port, "");
        var token = "";
        foreach (var m in groupInfo.members)
        {
            if (m.Key == avatarID)
            {
                token = m.Value.token;
            }
        }
        if (token == null)
        {
            Debug.LogError("");
        }
        _sceneSession._onConnect = (Action)delegate ()
        {
            _sceneSession.Send(new Proto4z.AttachSceneReq(avatarID, groupInfo.sceneID, token));
        };
        _sceneSession.Connect();
    }
	void OnSceneGroupInfoNotice(SceneGroupInfoNotice notice)
	{
        Debug.Log(notice);
		if (Facade.GroupInfo != null 
			&& Facade.GroupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_ACTIVE
			&& notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_NONE) 
		{
			GameObject.Destroy (_scene);
			if (_sceneSession != null) 
			{
				_sceneSession.Close ();
				_sceneSession = null;
			}
		}
		if (Facade.GroupInfo != null 
			&& Facade.GroupInfo.sceneState != (UInt16)SceneState.SCENE_STATE_WAIT
			&& notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_WAIT) 
		{
            CreateSceneSession(Facade.AvatarInfo.avatarID, notice.groupInfo);
        }

		Facade.GroupInfo = notice.groupInfo;
		Debug.Log (notice);
		if (Facade.GroupInfo.groupID == 0)
		{
			_client.Send (new Proto4z.SceneGroupCreateReq ());
		}

	}

	void OnSceneGroupGetResp(SceneGroupGetResp resp)
	{
        Debug.Log(resp);
    }
    void OnSceneGroupEnterResp(SceneGroupEnterResp resp)
	{
        Debug.Log(resp);
	}
	void OnSceneGroupCancelResp(SceneGroupCancelResp resp)
	{
        Debug.Log(resp);
    }
    void OnSceneGroupCreateResp(SceneGroupCreateResp resp)
	{
        Debug.Log(resp);
    }
    void OnSceneGroupLeaveResp(SceneGroupLeaveResp resp)
	{
        Debug.Log(resp);
    }

    void OnExitScene()
	{
		if (Facade.GroupInfo == null) 
		{
			return;
		}
		if (Facade.GroupInfo.sceneState != (ushort)SceneState.SCENE_STATE_ACTIVE 
			&& Facade.GroupInfo.sceneType != (ushort)SceneType.SCENE_HOME) 
		{
			return;
		}
		_client.Send (new SceneGroupCancelReq ());
	}

	void OnHomeScene()
	{
		if (Facade.GroupInfo == null) 
		{
			return;
		}
		if (Facade.GroupInfo.sceneState != (ushort)SceneState.SCENE_STATE_NONE && Facade.GroupInfo.sceneType != (ushort)SceneType.SCENE_HOME) 
		{
			return;
		}
		if (Facade.GroupInfo.groupID == 0) 
		{
			return;
		}
        if (Facade.GroupInfo.sceneState == (ushort)SceneState.SCENE_STATE_ACTIVE)
        {
            CreateSceneSession(Facade.AvatarInfo.avatarID, Facade.GroupInfo);
        }
        else
        {
            _client.Send(new Proto4z.SceneGroupEnterReq((ushort)SceneType.SCENE_HOME, 0));
        }
    }
	void OnArenaScene()
	{
	}

    void OnAvatarBaseInfoNotice(AvatarBaseInfoNotice resp)
    {
        Debug.logger.Log("NetController::AvatarBaseInfoNotice " + resp.baseInfo.avatarName);
        if (resp.baseInfo.avatarID == Facade.AvatarInfo.avatarID)
        {
            Facade.AvatarInfo = resp.baseInfo;
        }
    }
    void OnPingPongResp(PingPongResp resp)
    {
        //Debug.logger.Log("NetController::PingPongResp " + resp.msg);
        Invoke("PingPongSend", 5.0f);
    }
    void PingPongSend()
    {
        _client.Send(new PingPongReq("curtime=" + Time.realtimeSinceStartup));
    }



    void OnSceneSectionNotice(SceneSectionNotice notice)
    {
        Debug.Log(notice);
        if (_scene != null)
        {
            GameObject.Destroy(_scene.gameObject);
        }
        var scene = Resources.Load<GameObject>("Scene/Home");
        if (scene != null)
        {
            Debug.Log("create scene");
            _scene = Instantiate(scene).transform;
            _scene.gameObject.SetActive(true);
            var ugui = GameObject.Find("MainUI");
            var bg = ugui.GetComponent<RawImage>();
            bg.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("can't Instantiate [Prefabs/Guis/SelectScene/SelectScene].");
        }
    }
    void OnSceneRefreshNotice(SceneRefreshNotice notice)
    {
        Debug.Log(notice);
    }
    void OnAddEntityNotice(AddEntityNotice notice)
    {
        Debug.Log(notice);
        foreach (var entity in notice.entitys)
        {
            Facade.CreateAvatar(entity.baseInfo.modeID);
        }
    }
    void OnRemoveEntityNotice(RemoveEntityNotice notice)
    {
        Debug.Log(notice);
    }
    void OnMoveNotice(MoveNotice notice)
    {
        Debug.Log(notice);
    }
    void OnAddBuffNotice(AddBuffNotice notice)
    {
        Debug.Log(notice);
    }
    void OnRemoveBuffNotice(RemoveBuffNotice notice)
    {
        Debug.Log(notice);
    }
    void OnUseSkillNotice(UseSkillNotice notice)
    {
        Debug.Log(notice);
    }
    void OnMoveResp(MoveResp resp)
    {
        Debug.Log(resp);
    }
    void OnUseSkillResp(UseSkillResp resp)
    {
        Debug.Log(resp);
    }
    void OnAttachSceneResp(AttachSceneResp resp)
    {
        Debug.Log(resp);
    }




    // Update is called once per frame
    void Update()
    {
        if (_client != null)
        {
            _client.Update();
        }
        if (_sceneSession != null)
        {
            _sceneSession.Update();
        }
        if (_busyTips != null)
        {
            if (_client != null)
            {
                if (_client.Status == SessionStatus.SS_CONNECTING || _client.Status == SessionStatus.SS_INITING)
                {
                    if (!_busyTips.activeSelf)
                    {
                        _busyTips.SetActive(true);
                    }
                }
                else if (_busyTips.activeSelf)
                {
                    _busyTips.SetActive(false);
                }
            }
            else if (_busyTips.activeSelf)
            {
                _busyTips.SetActive(false);
            }

        }
    }

}
