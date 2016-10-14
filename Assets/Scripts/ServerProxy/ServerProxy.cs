using UnityEngine;
using Proto4z;
using System.Collections;
using UnityEngine.UI;
using System;

public class ServerProxy : MonoBehaviour
{

    Session _client;
    float _clientLastPulse = 0.0f;
	float _clientLastPing = 0.0f;
    float _clientPingValue = 0.0f;


    float _lastFPSTime = 0.0f;
    float _frameCount = 0.0f;
    float _lastFPS = 0.0f;

    Session _sceneSession;
    float _sceneSessionLastPulse = 0.0f;
    public SessionStatus ClientStatus { get { return _client == null ?  SessionStatus.SS_UNINIT: _client.Status; } }
    string _account;
    string _passwd;
    GameObject _busyTips;
    GameObject _chatPanel;
	Transform _scene = null;



    void Awake()
    {
        Debug.Log("ServerProxy Awake");
        DontDestroyOnLoad(gameObject);
        _busyTips = GameObject.Find("BusyTips");
        _chatPanel = GameObject.Find("ChatUI");
        Facade.GetSingleton<Dispatcher>().AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade.GetSingleton<Dispatcher>().AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AvatarBaseInfoNotice", (System.Action<AvatarBaseInfoNotice>)OnAvatarBaseInfoNotice);
        Facade.GetSingleton<Dispatcher>().AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);
        Facade.GetSingleton<Dispatcher>().AddListener("SceneClientPulse", (System.Action<SceneClientPulse>)OnSceneClientPulse);
        Facade.GetSingleton<Dispatcher>().AddListener("ClientPulse", (System.Action<ClientPulse>)OnClientPulse);


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

        Facade.GetSingleton<Dispatcher>().AddListener("OnArenaScene", (System.Action)OnArenaScene);
        Facade.GetSingleton<Dispatcher>().AddListener("OnHomeScene", (System.Action)OnHomeScene);
        Facade.GetSingleton<Dispatcher>().AddListener("OnExitScene", (System.Action)OnExitScene);

		Facade.GetSingleton<Dispatcher>().AddListener("ClientPingTestResp", (System.Action<ClientPingTestResp>)OnClientPingTestResp);



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
        _clientLastPulse = Time.realtimeSinceStartup;
        
    }
    public void OnConnect()
    {
        _client.Send(new ClientAuthReq(_account, _passwd));
        if (_chatPanel !=null && !_chatPanel.activeSelf)
        {
            _chatPanel.SetActive(true);
        }
    }

    public void SendToGame<T>(T proto) where T : Proto4z.IProtoObject
    {
        _client.Send(proto);
    }
    public void SendToScene<T>(T proto) where T : Proto4z.IProtoObject
    {
        _sceneSession.Send(proto);
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
            Debug.logger.Log(LogType.Error, "ServerProxy::OnCreateAvatarResp ");
            return;
        }
        Debug.logger.Log("ServerProxy::OnCreateAvatarResp ");
        _client.Send(new AttachAvatarReq("", resp.previews[0].avatarID));
    }

    void OnAttachAvatarResp(AttachAvatarResp resp)
    {
        Debug.logger.Log("ServerProxy::AttachAvatarResp ");
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS )
        {
            Debug.LogError("ServerProxy::AttachAvatarResp ");
            return;
        }
        if (!Facade._mainUI._selectScenePanel.gameObject.activeSelf)
        {
            Facade._mainUI._selectScenePanel.gameObject.SetActive(true);
        }
        Facade._avatarInfo = resp.baseInfo;
		_client.Send (new SceneGroupGetReq ());


        if (Facade._mainUI._loginUI.gameObject.activeSelf)
        {
            Facade._mainUI._loginUI.gameObject.SetActive(false);
        }
    }
    void CreateSceneSession(ulong avatarID,  Proto4z.SceneGroupInfo groupInfo)
    {
        _sceneSession = new Session();
        _sceneSession.Init(groupInfo.host, groupInfo.port, "");
        _sceneSessionLastPulse = Time.realtimeSinceStartup;
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
		if (Facade._groupInfo != null 
			&& Facade._groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_ACTIVE
			&& notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_NONE) 
		{
			GameObject.Destroy (_scene.gameObject);
            _scene = null;
            Facade._gameScene.CleanEntity();
            Facade._mainUI._skillPanel.gameObject.SetActive(false);
            Facade._mainUI._touchPanel.gameObject.SetActive(false);
            Facade._mainUI.SetActiveBG(true);
			if (_sceneSession != null) 
			{
				_sceneSession.Close ();
				_sceneSession = null;
			}
		}
		if (Facade._groupInfo != null 
			&& Facade._groupInfo.sceneState != (UInt16)SceneState.SCENE_STATE_WAIT
			&& notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_WAIT) 
		{
            CreateSceneSession(Facade._avatarInfo.avatarID, notice.groupInfo);
        }

		Facade._groupInfo = notice.groupInfo;
		Debug.Log (notice);
		if (Facade._groupInfo.groupID == 0)
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
		if (Facade._groupInfo == null) 
		{
			return;
		}
		if (Facade._groupInfo.sceneState != (ushort)SceneState.SCENE_STATE_ACTIVE 
			&& Facade._groupInfo.sceneType != (ushort)SceneType.SCENE_HOME) 
		{
			return;
		}
		_client.Send (new SceneGroupCancelReq ());
	}

	void OnHomeScene()
	{
		if (Facade._groupInfo == null) 
		{
			return;
		}
		if (Facade._groupInfo.sceneState != (ushort)SceneState.SCENE_STATE_NONE && Facade._groupInfo.sceneType != (ushort)SceneType.SCENE_HOME) 
		{
			return;
		}
		if (Facade._groupInfo.groupID == 0) 
		{
			return;
		}
        if (Facade._groupInfo.sceneState == (ushort)SceneState.SCENE_STATE_ACTIVE)
        {
            CreateSceneSession(Facade._avatarInfo.avatarID, Facade._groupInfo);
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
        Debug.logger.Log("ServerProxy::AvatarBaseInfoNotice " + resp.baseInfo.avatarName);
        if (resp.baseInfo.avatarID == Facade._avatarInfo.avatarID)
        {
            Facade._avatarInfo = resp.baseInfo;
        }
    }
    void OnPingPongResp(PingPongResp resp)
    {
        //Debug.logger.Log("ServerProxy::PingPongResp " + resp.msg);
        Invoke("PingPongSend", 5.0f);
    }
    void OnSceneClientPulse(SceneClientPulse resp)
    {
    }
    void OnClientPulse(ClientPulse resp)
    {
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
            Facade._mainUI.SetActiveBG(false);
            Facade._mainUI._touchPanel.gameObject.SetActive(true);
            Facade._mainUI._skillPanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("can't Instantiate [Prefabs/Guis/SelectScene/SelectScene].");
        }
    }
    void OnSceneRefreshNotice(SceneRefreshNotice notice)
    {
        Debug.Log(notice);
        Facade._gameScene.RefreshEntityInfo(notice.entityInfos);
        Facade._gameScene.RefreshEntityMove(notice.entityMoves);
    }
    void OnAddEntityNotice(AddEntityNotice notice)
    {
        Debug.Log(notice);
        foreach (var entity in notice.entitys)
        {
            Facade._gameScene.CreateEntity(entity);
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
	void OnClientPingTestResp(ClientPingTestResp resp)
	{
        _clientPingValue = Time.realtimeSinceStartup - (float)resp.clientTime;
        Debug.Log("Ping:" + _clientPingValue);
	}


    void OnGUI()
    {
        string name = "Ping:" + _clientPingValue;
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(name));
        Vector2 position = new Vector2(Screen.height / 30, Screen.width / 40);
        GUI.color = Color.red;
        if (_scene != null)
        {
            GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name);
        }
        position.y += nameSize.y;
  
        name = "FPS:" + _lastFPS;
        GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name);
    }
    // Update is called once per frame
    void Update()
    {
        _frameCount++;
        if (Time.realtimeSinceStartup - _lastFPSTime > 1.0f)
        {
            _lastFPS = _frameCount;
            _frameCount = 0;
            _lastFPSTime = Time.realtimeSinceStartup;
        }
		if (_sceneSession != null && Time.realtimeSinceStartup - _clientLastPing >5.0f) 
		{
			_clientLastPing = Time.realtimeSinceStartup;
			_sceneSession.Send(new Proto4z.ClientPingTestReq (0, _clientLastPing));
		}
        if (_client != null)
        {
            _client.Update();
            if (Time.realtimeSinceStartup - _clientLastPulse > 30.0f)
            {
                _clientLastPulse = Time.realtimeSinceStartup;
                _client.Send<Proto4z.ClientPulse>(new Proto4z.ClientPulse());
            }
        }
        if (_sceneSession != null)
        {
            _sceneSession.Update();
            if (Time.realtimeSinceStartup - _sceneSessionLastPulse > 30.0f)
            {
                _sceneSessionLastPulse = Time.realtimeSinceStartup;
                _client.Send<Proto4z.SceneClientPulse>(new Proto4z.SceneClientPulse());
            }
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
