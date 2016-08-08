using UnityEngine;
using Proto4z;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unistar;

public  class Dispatcher : MonoBehaviour
{
    System.Collections.Generic.Dictionary<string, Delegate> _routing = new System.Collections.Generic.Dictionary<string, Delegate>();
    public void AddListener(string method, Delegate dlg)
    {
        if (_routing.ContainsKey(method))
        {
            Debug.logger.Log(LogType.Error, "Dispatcher::AddListener duplicate method name = " + method);
        }
        _routing.Add(method, dlg);
    }
    public void RemoveListener(string method)
    {
        if (!_routing.ContainsKey(method))
        {
            Debug.logger.Log(LogType.Error, "Dispatcher::RemoveListener not found method name = " + method);
        }
        _routing.Remove(method);
    }
    public void TriggerEvent(string method, object[] args)
    {
        Delegate dlg;
        if (!_routing.TryGetValue(method, out dlg) || dlg == null)
        {
            Debug.logger.Log(LogType.Error, "Dispatcher::TriggerEvent not found method. name = " + method);
            return;
        }
        dlg.DynamicInvoke(args);
    }
}
