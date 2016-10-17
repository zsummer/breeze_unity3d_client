using UnityEngine;
using Proto4z;
using System.Collections;
using UnityEngine.UI;
using System;

public class ServerProxy : MonoBehaviour
{

    Session _gameClient;
	float _gameLastPulse = 0.0f;


    float _lastFPSTime = 0.0f;
    float _frameCount = 0.0f;
    float _fpsValue = 0.0f;

    Session _sceneClient;
    float _sceneLastPulse = 0.0f;
    float _sceneLastPing = 0.0f;
    float _scenePingValue = 0.0f;

    string _account;
    string _passwd;




    void Awake()
    {
        Debug.Log("ServerProxy Awake");
        DontDestroyOnLoad(gameObject);
        Facade._dispatcher.AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade._dispatcher.AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade._dispatcher.AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade._dispatcher.AddListener("AvatarBaseInfoNotice", (System.Action<AvatarBaseInfoNotice>)OnAvatarBaseInfoNotice);
        Facade._dispatcher.AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);
        Facade._dispatcher.AddListener("ClientPulse", (System.Action<ClientPulse>)OnClientPulse);

		Facade._dispatcher.AddListener("SceneClientPulse", (System.Action<SceneClientPulse>)OnSceneClientPulse);
		Facade._dispatcher.AddListener("ClientPingTestResp", (System.Action<ClientPingTestResp>)OnClientPingTestResp);

        Facade._dispatcher.AddListener("SceneGroupInfoNotice", (System.Action<SceneGroupInfoNotice>)OnSceneGroupInfoNotice);
        Facade._dispatcher.AddListener("SceneGroupGetResp", (System.Action<SceneGroupGetResp>)OnSceneGroupGetResp);
        Facade._dispatcher.AddListener("SceneGroupEnterResp", (System.Action<SceneGroupEnterResp>)OnSceneGroupEnterResp);
        Facade._dispatcher.AddListener("SceneGroupCancelResp", (System.Action<SceneGroupCancelResp>)OnSceneGroupCancelResp);

        Facade._dispatcher.AddListener("SceneGroupCreateResp", (System.Action<SceneGroupCreateResp>)OnSceneGroupCreateResp);
        Facade._dispatcher.AddListener("SceneGroupLeaveResp", (System.Action<SceneGroupLeaveResp>)OnSceneGroupLeaveResp);


        Facade._dispatcher.AddListener("AttachSceneResp", (System.Action<AttachSceneResp>)OnAttachSceneResp);

        Facade._dispatcher.AddListener("OnArenaScene", (System.Action)OnArenaScene);
        Facade._dispatcher.AddListener("OnHomeScene", (System.Action)OnHomeScene);
        Facade._dispatcher.AddListener("OnExitScene", (System.Action)OnExitScene);




    }
    void Start()
    {
        Debug.logger.Log("ServerProxy::Start ");
    }

    public void Login(string host, ushort port, string account, string pwd)
    {
        _account = account;
        _passwd = pwd;
        if (_gameClient != null)
        {
            _gameClient.Close();
        }
        _gameClient = new Session();
        _gameClient._onConnect = (Action)OnConnect;
        _gameClient.Init(host, port, "");
        _gameLastPulse = Time.realtimeSinceStartup;

    }
    public void OnConnect()
    {
        _gameClient.Send(new ClientAuthReq(_account, _passwd));
    }

    public void SendToGame<T>(T proto) where T : Proto4z.IProtoObject
    {
        _gameClient.Send(proto);
    }
    public void SendToScene<T>(T proto) where T : Proto4z.IProtoObject
    {
        _sceneClient.Send(proto);
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
            _gameClient.Send(new CreateAvatarReq("", _account));
        }
        else
        {
            _gameClient.Send(new AttachAvatarReq("", resp.previews[0].avatarID));
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
        _gameClient.Send(new AttachAvatarReq("", resp.previews[0].avatarID));
    }

    void OnAttachAvatarResp(AttachAvatarResp resp)
    {
        Debug.logger.Log("ServerProxy::AttachAvatarResp ");
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS)
        {
            Debug.LogError("ServerProxy::AttachAvatarResp ");
            return;
        }

        Facade._avatarInfo = resp.baseInfo;
        _gameClient.Send(new SceneGroupGetReq());

        if (Facade._mainUI._loginUI.gameObject.activeSelf)
        {
            Facade._mainUI._loginUI.gameObject.SetActive(false);
        }
        if (!Facade._mainUI._chatUI.gameObject.activeSelf)
        {
            Facade._mainUI._chatUI.gameObject.SetActive(true);
        }
        if (!Facade._mainUI._selectScenePanel.gameObject.activeSelf)
        {
            Facade._mainUI._selectScenePanel.gameObject.SetActive(true);
        }
    }
    void CreateSceneSession(ulong avatarID, Proto4z.SceneGroupInfo groupInfo)
    {
        _sceneClient = new Session();
        _sceneClient.Init(groupInfo.host, groupInfo.port, "");
        _sceneLastPulse = Time.realtimeSinceStartup;
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
        _sceneClient._onConnect = (Action)delegate ()
        {
            _sceneClient.Send(new Proto4z.AttachSceneReq(avatarID, groupInfo.sceneID, token));
        };
        _sceneClient.Connect();
    }
    void OnSceneGroupInfoNotice(SceneGroupInfoNotice notice)
    {
        if (Facade._groupInfo != null
            && Facade._groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_ACTIVE
            && notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_NONE)
        {
			Facade._sceneManager.DestroyCurrentScene ();
            if (_sceneClient != null)
            {
                _sceneClient.Close();
                _sceneClient = null;
            }
        }
        if (Facade._groupInfo != null
            && Facade._groupInfo.sceneState != (UInt16)SceneState.SCENE_STATE_WAIT
            && notice.groupInfo.sceneState == (UInt16)SceneState.SCENE_STATE_WAIT)
        {
            CreateSceneSession(Facade._avatarInfo.avatarID, notice.groupInfo);
        }

        Facade._groupInfo = notice.groupInfo;
        Debug.Log(notice);
        if (Facade._groupInfo.groupID == 0)
        {
            _gameClient.Send(new Proto4z.SceneGroupCreateReq());
        }

    }

    void OnSceneGroupGetResp(SceneGroupGetResp resp)
    {
    }
    void OnSceneGroupEnterResp(SceneGroupEnterResp resp)
    {
        
    }
    void OnSceneGroupCancelResp(SceneGroupCancelResp resp)
    {
        
    }
    void OnSceneGroupCreateResp(SceneGroupCreateResp resp)
    {
        
    }
    void OnSceneGroupLeaveResp(SceneGroupLeaveResp resp)
    {
        
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
        _gameClient.Send(new SceneGroupCancelReq());
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
            _gameClient.Send(new Proto4z.SceneGroupEnterReq((ushort)SceneType.SCENE_HOME, 0));
        }
    }
    void OnArenaScene()
    {
        if (Facade._groupInfo == null)
        {
            return;
        }
        if (Facade._groupInfo.sceneState != (ushort)SceneState.SCENE_STATE_NONE && Facade._groupInfo.sceneType != (ushort)SceneType.SCENE_ARENA)
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
            _gameClient.Send(new Proto4z.SceneGroupEnterReq((ushort)SceneType.SCENE_ARENA, 0));
        }
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
        _gameClient.Send(new PingPongReq("curtime=" + Time.realtimeSinceStartup));
    }

	void OnAttachSceneResp(AttachSceneResp resp)
	{
		
	}


    void OnGUI()
    {
        string name;
        GUIStyle st = new GUIStyle();
        st.normal.textColor = Color.red;
        st.normal.background = null;
        st.fontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);
        Vector2 nameSize;
        Vector2 position = new Vector2(st.fontSize, st.fontSize);

		name = "屏幕大小:" + Screen.width + "*" + Screen.height;
		nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
		GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);

		name = "系统日期:" + System.DateTime.Now;
		nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
		position.y += nameSize.y;
		GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);

		name = "FPS:" + _fpsValue;
		nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
		position.y += nameSize.y;
		GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);

		if (Facade._avatarInfo != null) 
		{
			var modelID = Facade._avatarInfo.modeID;
			name = "角色模型[" + modelID +"]:" + Facade._modelDict.GetModelName(modelID);
			nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
			position.y += nameSize.y;
			GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);
		}

		if (Facade._avatarInfo != null && Facade._entityID != 0) 
		{
			var modelID = Facade._sceneManager.GetEntity (Facade._entityID)._info.baseInfo.modeID;
			name = "当前模型[" + modelID +"]:" + Facade._modelDict.GetModelName(modelID);
			nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
			position.y += nameSize.y;
			GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);
		}



        if (Facade._groupInfo == null)
        {
            name = "当前位置:主界面";
        }
        else if (Facade._groupInfo.sceneType == (ushort)SceneType.SCENE_NONE)
        {
            name = "当前位置:主界面";
        }
        else if (Facade._groupInfo.sceneType == (ushort)SceneType.SCENE_HOME)
        {
            name = "当前位置:主城";
        }
        else
        {
            name = "当前位置:战场";
        }
        nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
		position.y += nameSize.y;
        GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);


		if (Facade._entityID != 0)
        {
            name = "Ping:" + _scenePingValue +"秒";
            nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
            position.y += nameSize.y;
            GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);

			name = "场景过期:" + Facade._sceneManager.GetSceneCountdown() + "秒" ;
            nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
            position.y += nameSize.y;
            GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);
        }
        name = "about: zsummer";
        nameSize = GUI.skin.label.CalcSize(new GUIContent(name)) * st.fontSize / GUI.skin.font.fontSize;
        position.y += nameSize.y;
        GUI.Label(new Rect(position.x, position.y, nameSize.x, nameSize.y), name, st);
    }

	void OnClientPingTestResp(ClientPingTestResp resp)
	{
		_scenePingValue = Time.realtimeSinceStartup - (float)resp.clientTime;
	}


    // Update is called once per frame
    void Update()
    {
        _frameCount++;
        if (Time.realtimeSinceStartup - _lastFPSTime > 1.0f)
        {
            _fpsValue = _frameCount;
            _frameCount = 0;
            _lastFPSTime = Time.realtimeSinceStartup;
        }

        if (_gameClient != null)
        {
            _gameClient.Update();
            if (Time.realtimeSinceStartup - _gameLastPulse > 30.0f)
            {
                _gameLastPulse = Time.realtimeSinceStartup;
                _gameClient.Send<Proto4z.ClientPulse>(new Proto4z.ClientPulse());
            }
        }
        if (_sceneClient != null)
        {
            _sceneClient.Update();
            if (Time.realtimeSinceStartup - _sceneLastPulse > 30.0f)
            {
                _sceneLastPulse = Time.realtimeSinceStartup;
                _gameClient.Send<Proto4z.SceneClientPulse>(new Proto4z.SceneClientPulse());
            }
			if (Time.realtimeSinceStartup - _sceneLastPing > 5.0f) 
			{
				_sceneLastPing = Time.realtimeSinceStartup;
				_sceneClient.Send (new Proto4z.ClientPingTestReq (0, Time.realtimeSinceStartup));
			}
        }

        if ((_gameClient != null && (_gameClient.Status == SessionStatus.SS_CONNECTING || _gameClient.Status == SessionStatus.SS_INITING))
            || (_sceneClient != null && (_sceneClient.Status == SessionStatus.SS_CONNECTING || _sceneClient.Status == SessionStatus.SS_INITING))
            )
        {
            if (!Facade._mainUI._busyTips.gameObject.activeSelf)
            {
                Facade._mainUI._busyTips.gameObject.SetActive(true);
            }
        }
        else if (Facade._mainUI._busyTips.gameObject.activeSelf)
        {
            Facade._mainUI._busyTips.gameObject.SetActive(false);
        }

    }

}
