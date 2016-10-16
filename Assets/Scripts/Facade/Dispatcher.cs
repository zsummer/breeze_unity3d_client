using UnityEngine;
using Proto4z;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public  class Dispatcher : MonoBehaviour
{
    System.Collections.Generic.Dictionary<string, Delegate> _routing = new System.Collections.Generic.Dictionary<string, Delegate>();
    void Awake()
    {
        Debug.Log("Awake Dispatcher.");
        DontDestroyOnLoad(gameObject);
    }

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
            Debug.LogError("Dispatcher::RemoveListener not found method name = " + method);
        }
        _routing.Remove(method);
    }
    public void TriggerEvent(string method, object[] args)
    {
        if (false)
        {
            string info = "Dispatcher::TriggerEvent call [" + method + "](";
            if (args != null)
            {
                foreach (var arg in args)
                {
                    info += arg + ",";
                }
            }
            info += ").";
            Debug.Log(info);
        }
        try
        {
            Delegate dlg;
            if (!_routing.TryGetValue(method, out dlg) || dlg == null)
            {
                throw new Exception("the method no listener.");
            }
            dlg.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            string err = "Dispatcher::TriggerEvent [" + method + "](";
            if (args != null)
            {
                foreach (var arg in args)
                {
                    err += arg + ",";
                }
            }
            err += ") had Exception:";
            err += e.Message;
            Debug.LogError(err);
        }
        
    }
}
