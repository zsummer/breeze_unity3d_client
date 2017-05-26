using UnityEngine;
using Proto4z;
using System.Collections;
using UnityEngine.UI;
using System;





public class ServerProxy : MonoBehaviour
{

    Session _gameClient;
    string _gameHost;
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
        Facade.dispatcher.AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade.dispatcher.AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade.dispatcher.AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade.dispatcher.AddListener("AvatarBaseInfoNotice", (System.Action<AvatarBaseInfoNotice>)OnAvatarBaseInfoNotice);
        Facade.dispatcher.AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);
        Facade.dispatcher.AddListener("ClientPulse", (System.Action<ClientPulse>)OnClientPulse);

		Facade.dispatcher.AddListener("SceneClientPulse", (System.Action<SceneClientPulse>)OnSceneClientPulse);
		Facade.dispatcher.AddListener("ClientPingTestResp", (System.Action<ClientPingTestResp>)OnClientPingTestResp);

        Facade.dispatcher.AddListener("SceneGroupInfoNotice", (System.Action<SceneGroupInfoNotice>)OnSceneGroupInfoNotice);
        Facade.dispatcher.AddListener("SceneGroupGetResp", (System.Action<SceneGroupGetResp>)OnSceneGroupGetResp);
        Facade.dispatcher.AddListener("SceneGroupEnterResp", (System.Action<SceneGroupEnterResp>)OnSceneGroupEnterResp);
        Facade.dispatcher.AddListener("SceneGroupCancelResp", (System.Action<SceneGroupCancelResp>)OnSceneGroupCancelResp);

        Facade.dispatcher.AddListener("SceneGroupCreateResp", (System.Action<SceneGroupCreateResp>)OnSceneGroupCreateResp);
        Facade.dispatcher.AddListener("SceneGroupLeaveResp", (System.Action<SceneGroupLeaveResp>)OnSceneGroupLeaveResp);


        Facade.dispatcher.AddListener("AttachSceneResp", (System.Action<AttachSceneResp>)OnAttachSceneResp);

        Facade.dispatcher.AddListener("ClickArenaScene", (System.Action)ClickArenaScene);
        Facade.dispatcher.AddListener("ClickHomeScene", (System.Action)ClickHomeScene);
        Facade.dispatcher.AddListener("ClickMeleeScene", (System.Action)ClickMeleeScene);
        Facade.dispatcher.AddListener("ClickExitScene", (System.Action)ClickExitScene);




    }
    void Start()
    {
        Debug.logger.Log("ServerProxy::Start ");
    }

    public void Login(string host, ushort port, string account, string pwd)
    {
        _account = account;
        _passwd = pwd;
        _gameHost = host;
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

        Facade.avatarInfo = resp.baseInfo;
        _gameClient.Send(new SceneGroupGetReq());

        if (Facade.mainUI._loginUI.gameObject.activeSelf)
        {
            Facade.mainUI._loginUI.gameObject.SetActive(false);
        }
        if (!Facade.mainUI._chatUI.gameObject.activeSelf)
        {
            Facade.mainUI._chatUI.gameObject.SetActive(true);
        }
        if (!Facade.mainUI._selectScenePanel.gameObject.activeSelf)
        {
            Facade.mainUI._selectScenePanel.gameObject.SetActive(true);
        }
        MainScreenLabel.Bulletin(1, "登录时间:" + DateTime.Now.ToString());
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
        if (Facade.sceneState != null
            && Facade.sceneState.sceneState == (UInt16)SCENE_STATE.SCENE_STATE_ACTIVE
            && notice.groupInfo.sceneState == (UInt16)SCENE_STATE.SCENE_STATE_NONE)
        {
			Facade.sceneManager.DestroyCurrentScene ();
            if (_sceneClient != null)
            {
                _sceneClient.Close();
                _sceneClient = null;
            }
        }
        if (Facade.sceneState != null
            && Facade.sceneState.sceneState != (UInt16)SCENE_STATE.SCENE_STATE_WAIT
            && notice.groupInfo.sceneState == (UInt16)SCENE_STATE.SCENE_STATE_WAIT)
        {
            //check debug mode  
            if (notice.groupInfo.host.Length == 0 || notice.groupInfo.host == "localhost" || notice.groupInfo.host == "127.0.0.1")
            {
                if (_gameHost != null && _gameHost.Length > 0)
                {
                    notice.groupInfo.host = _gameHost;
                }
            }
            CreateSceneSession(Facade.avatarInfo.avatarID, notice.groupInfo);
        }

        Facade.sceneState = notice.groupInfo;
        Debug.Log(notice);
        if (Facade.sceneState.groupID == 0)
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
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS)
        {

        }
    }
    void OnSceneGroupCreateResp(SceneGroupCreateResp resp)
    {
        
    }
    void OnSceneGroupLeaveResp(SceneGroupLeaveResp resp)
    {
        
    }

    void ClickExitScene()
    {
        if (Facade.sceneState == null)
        {
            return;
        }
        if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_MATCHING 
            || ( Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ACTIVE
                        && (Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_HOME
                               || Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_MELEE)
                 )
             )
        {
            _gameClient.Send(new SceneGroupCancelReq());
        }

       
    }

    void ClickHomeScene()
    {
        if (Facade.sceneState == null)
        {
            return;
        }
        if (Facade.sceneState.sceneState != (ushort)SCENE_STATE.SCENE_STATE_NONE && Facade.sceneState.sceneType != (ushort)SCENE_TYPE.SCENE_HOME)
        {
            return;
        }
        if (Facade.sceneState.groupID == 0)
        {
            return;
        }
        if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ACTIVE)
        {
            CreateSceneSession(Facade.avatarInfo.avatarID, Facade.sceneState);
        }
        else
        {
            _gameClient.Send(new Proto4z.SceneGroupEnterReq((ushort)SCENE_TYPE.SCENE_HOME, 0));
        }
    }

    void ClickMeleeScene()
    {
        if (Facade.sceneState == null)
        {
            return;
        }
        if (Facade.sceneState.sceneState != (ushort)SCENE_STATE.SCENE_STATE_NONE && Facade.sceneState.sceneType != (ushort)SCENE_TYPE.SCENE_MELEE)
        {
            return;
        }
        if (Facade.sceneState.groupID == 0)
        {
            return;
        }
        if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ACTIVE)
        {
            CreateSceneSession(Facade.avatarInfo.avatarID, Facade.sceneState);
        }
        else
        {
            _gameClient.Send(new Proto4z.SceneGroupEnterReq((ushort)SCENE_TYPE.SCENE_MELEE, 0));
        }
    }


    void ClickArenaScene()
    {
        if (Facade.sceneState == null)
        {
            return;
        }
        if (Facade.sceneState.sceneState != (ushort)SCENE_STATE.SCENE_STATE_NONE && Facade.sceneState.sceneType != (ushort)SCENE_TYPE.SCENE_ARENA)
        {
            return;
        }
        if (Facade.sceneState.groupID == 0)
        {
            return;
        }
        if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ACTIVE)
        {
            CreateSceneSession(Facade.avatarInfo.avatarID, Facade.sceneState);
        }
        else
        {
            _gameClient.Send(new Proto4z.SceneGroupEnterReq((ushort)SCENE_TYPE.SCENE_ARENA, 0));
        }
    }

    void OnAvatarBaseInfoNotice(AvatarBaseInfoNotice resp)
    {
        Debug.logger.Log("ServerProxy::AvatarBaseInfoNotice " + resp.baseInfo.avatarName);
        if (resp.baseInfo.avatarID == Facade.avatarInfo.avatarID)
        {
            Facade.avatarInfo = resp.baseInfo;
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
        if (resp.retCode == (ushort)Proto4z.ERROR_CODE.EC_SUCCESS)
        {
            Debug.logger.Log("ServerProxy::OnAttachSceneResp sucess. sceneID=" +  resp.sceneID.ToString());
        }
        else
        {
            Debug.logger.Log("ServerProxy::OnAttachSceneResp sucess. error code=" + resp.retCode.ToString() + ", sceneID=" + resp.sceneID.ToString());
        }
        
    }


    void OnGUI()
    {
        string name;
        MainScreenLabel.Preprocess();

		name = "屏幕大小:" + Screen.width + "*" + Screen.height;
        MainScreenLabel.Label(name);



		name = "系统日期:" + System.DateTime.Now;
        MainScreenLabel.Label(name);

        name = "FPS:" + _fpsValue;
        MainScreenLabel.Label(name);

        if (Facade.avatarInfo != null) 
		{
			var modelID = Facade.avatarInfo.modeID;
			name = "角色模型[" + modelID +"]:" + Facade.modelDict.GetModelName(modelID);
            MainScreenLabel.Label(name);
        }

		if (Facade.avatarInfo != null && Facade.myShell != 0) 
		{
			var modelID = Facade.sceneManager.GetShell (Facade.myShell).ghost.state.modelID;
			name = "当前模型[" + modelID +"]:" + Facade.modelDict.GetModelName(modelID);
            MainScreenLabel.Label(name);
        }



        if (Facade.sceneState == null)
        {
            name = "当前位置:主界面";
        }
        else if (Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_NONE)
        {
            name = "当前位置:主界面";
        }
        else if (Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_HOME)
        {
            name = "当前位置:主城";
        }
        else if (Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_MELEE)
        {
            name = "当前位置:乱斗场";
        }
        else if (Facade.sceneState.sceneType == (ushort)SCENE_TYPE.SCENE_ARENA)
        {
            name = "当前位置:竞技场";
        }
        else
        {
            name = "当前位置:未命名战场";
        }
        MainScreenLabel.Label(name);

        if (Facade.sceneState == null || Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_NONE)
        {
            name = "当前状态:闲置";
        }
        else if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_MATCHING)
        {
            name = "当前状态:匹配中";
        }
        else if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ALLOCATE)
        {
            name = "当前状态:场景调度";
        }
        else if (Facade.sceneState.sceneState == (ushort)SCENE_STATE.SCENE_STATE_ACTIVE)
        {
            name = "当前状态:游戏中";
        }
        else
        {
            name = "当前状态:其他状态";
        }
        MainScreenLabel.Label(name);



        if (Facade.myShell != 0)
        {
            name = "Ping:" + _scenePingValue +"秒";
            MainScreenLabel.Label(name);

            name = "场景过期:" + Facade.sceneManager.GetSceneCountdown() + "秒" ;
            MainScreenLabel.Label(name);


            EntityShell em = Facade.sceneManager.GetShell(Facade.myShell);
            name = "坐标:" + em.ghost.mv.position.x.ToString("0.00") + ":" +em.ghost.mv.position.y.ToString("0.00");
            MainScreenLabel.Label(name);

            if (em.ghost.mv.action == (ushort)MOVE_ACTION.MOVE_ACTION_IDLE)
            {
                name = "静止";
            }
            else if (em.ghost.mv.action == (ushort)MOVE_ACTION.MOVE_ACTION_FOLLOW)
            {
                name = "跟随";
            }
            else if (em.ghost.mv.action == (ushort)MOVE_ACTION.MOVE_ACTION_PATH)
            {
                name = "移动";
            }
            else 
            {
                name = "被动位移";
            }
            name += ": real:" + em.ghost.mv.realSpeed.ToString("0.00") + " , expect" + em.ghost.mv.expectSpeed.ToString("0.00");
            MainScreenLabel.Label(name);

        }
        name = "about: zsummer";
        MainScreenLabel.Label(name);
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
            if (!Facade.mainUI._busyTips.gameObject.activeSelf)
            {
                Facade.mainUI._busyTips.gameObject.SetActive(true);
            }
        }
        else if (Facade.mainUI._busyTips.gameObject.activeSelf)
        {
            Facade.mainUI._busyTips.gameObject.SetActive(false);
        }

    }

}
