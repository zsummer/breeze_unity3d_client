using UnityEngine;
using Proto4z;
using System.Collections;
using System;
public class NetController : MonoBehaviour {

    Session _client;
    void Awake()
    {
        Debug.Log("Awake NetController.");
        DontDestroyOnLoad(gameObject);
        Facade.GetSingleton<Dispatcher>().AddListener("Login", (System.Action<string, ushort>)Login);
        Facade.GetSingleton<Dispatcher>().AddListener("SessionConnected", (System.Action<Session>)OnConnected);
        Facade.GetSingleton<Dispatcher>().AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade.GetSingleton<Dispatcher>().AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);
    }
    void Start()
    {
        Debug.logger.Log("NetController::Start ");
        Facade.GetSingleton<Dispatcher>().TriggerEvent("Login", new object[] { "127.0.0.1", (ushort)26001 });
    }

    void Login(string host, ushort port)
    {
        if (_client != null)
        {
            _client.Close(false);
        }
        _client = new Session();
        _client.Init(host, port, "");
        
    }
    void OnConnected(Session session)
    {
        if (_client == session)
        {
            _client.Send(new ClientAuthReq("test", "123"));
        }
    }

    void OnClientAuthResp(ClientAuthResp resp)
    {
        var account = resp.account;
        if (resp.retCode != (ushort)ERROR_CODE.EC_SUCCESS)
        {
            Debug.logger.Log(LogType.Error, "NetworkManager::OnClientAuthResp account=" + account);
            return;
        }
        Debug.logger.Log("NetController::OnClientAuthResp account=" + account);
        if (resp.previews.Count == 0)
        {
            _client.Send(new CreateAvatarReq("", "test1"));
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
            Debug.logger.Log(LogType.Error, "NetController::AttachAvatarResp ");
            return;
        }
        Debug.logger.Log("NetController::AttachAvatarResp ");
        PingPongSend();
    }

    void OnPingPongResp(PingPongResp resp)
    {
        Debug.logger.Log("NetController::PingPongResp " + resp.msg);
        Invoke("PingPongSend", 5.0f);
        
    }

    void PingPongSend()
    {
        _client.Send(new PingPongReq("curtime=" + Time.realtimeSinceStartup));
    }


    // Update is called once per frame
    void Update()
    {
        if (_client != null)
        {
            _client.Update();
        }
        
    }

}
