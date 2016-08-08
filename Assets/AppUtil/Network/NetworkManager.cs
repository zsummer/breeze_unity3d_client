using UnityEngine;
using Proto4z;
using System.Collections;
using System;
public class NetworkManager : MonoBehaviour {

    Session _client;
    void Start()
    {
        Debug.logger.Log("NetworkManager::Start ");
        _client = new Session();
        if (!_client.Init("127.0.0.1", 26001, ""))
        {
            //return;
        }

        Unistar.Singleton.getInstance<Dispatcher>().AddListener("ClientAuthResp", (System.Action< ClientAuthResp >) OnClientAuthResp);
        ClientAuthReq req = new ClientAuthReq("test", "123");
        _client.Send(req);
    }
    void OnClientAuthResp(ClientAuthResp resp)
    {
        var account = resp.account;
        Debug.logger.Log("NetworkManager::OnClientAuthResp account=" + account);
    }
    // Update is called once per frame
    void Update()
    {
        _client.Update();
    }

}
