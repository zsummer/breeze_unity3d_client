using UnityEngine;
using Proto4z;
using System.Collections;
using System;
public class NetController : MonoBehaviour
{

    Session _client;
    string _account;
    string _passwd;
    Delegate _onClosed;
    Delegate _onBeginConnect;
    Delegate _onEndConnect;
    void Awake()
    {
        Debug.Log("Awake NetController.");
        DontDestroyOnLoad(gameObject);
        
        Facade.GetSingleton<Dispatcher>().AddListener("ClientAuthResp", (System.Action<ClientAuthResp>)OnClientAuthResp);
        Facade.GetSingleton<Dispatcher>().AddListener("CreateAvatarResp", (System.Action<CreateAvatarResp>)OnCreateAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("AttachAvatarResp", (System.Action<AttachAvatarResp>)OnAttachAvatarResp);
        Facade.GetSingleton<Dispatcher>().AddListener("PingPongResp", (System.Action<PingPongResp>)OnPingPongResp);
    }
    void Start()
    {
        Debug.logger.Log("NetController::Start ");
        
        
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
        _client._onClosed = _onClosed;
        _client._onBeginConnect = _onBeginConnect;
        _client._onEndConnect = (Action<bool>)OnEndConnect;
        _client.Init(host, port, "");
        
    }
    public void OnEndConnect(bool isOK)
    {
        if (isOK)
        {
            _client.Send(new ClientAuthReq(_account, _passwd));
            if (_onEndConnect != null)
            {
                _onEndConnect.DynamicInvoke(isOK);
            }
        }
    }
    public void SetMainSessionDelegate(Action onClosed, Action onBeginConnect, Action<bool> onEndConnect)
    {
        _onClosed = onClosed;
        _onBeginConnect = onBeginConnect;
        _onEndConnect = onEndConnect;
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
            Debug.logger.Log(LogType.Error, "NetworkManager::OnClientAuthResp account=" + account);
            return;
        }
        Debug.logger.Log("NetController::OnClientAuthResp account=" + account);
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
            Debug.logger.Log(LogType.Error, "NetController::AttachAvatarResp ");
            return;
        }
        Debug.logger.Log("NetController::AttachAvatarResp ");
        PingPongSend();

        var login = GameObject.Find("LoginUI");
        if (login != null)
        {
            login.SetActive(false);
        }

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
