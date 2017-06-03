using UnityEngine;
using Proto4z;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;



public enum SessionStatus
{
    SS_UNINIT,
    SS_INITING,
    SS_INITED,
    SS_CONNECTING,
    SS_WORKING,
    SS_CLOSED,
}

class ProtoHeader : IProtoObject
{
    public const int HeadLen = 8;
    public int packLen;
    public ushort reserve;
    public ushort protoID;

    public System.Collections.Generic.List<byte> __encode()
    {
        var ret = new System.Collections.Generic.List<byte>();
        ret.AddRange(BaseProtoObject.encodeI32(packLen));
        ret.AddRange(BaseProtoObject.encodeUI16(reserve));
        ret.AddRange(BaseProtoObject.encodeUI16(protoID));
        return ret;
    }
    public System.Int32 __decode(byte[] binData, ref System.Int32 pos)
    {
        packLen = BaseProtoObject.decodeI32(binData, ref pos);
        reserve = BaseProtoObject.decodeUI16(binData, ref pos);
        protoID = BaseProtoObject.decodeUI16(binData, ref pos);
        return pos;
    }
}


class Session
{
    Socket _socket;
    string _sessionName;

    public SessionStatus state = SessionStatus.SS_UNINIT;
    public Action whenConnected;

    IPAddress _addr;
    ushort _port;
    int _reconnect = 0;
    float _lastConnectTime = 0.0f;
    float _lastRecvTime = 0.0f;
    const int MAX_BUFFER_SIZE = 200 * 1024;


    string _encrypt;

    RC4Encryption _rc4Send;
    private byte[] _sendBuffer;
    private int _sendBufferLen = 0;
    private System.Collections.Generic.Queue<byte[]> _sendQue;

    RC4Encryption _rc4Recv;
    private byte[] _recvBuffer;
    private int _recvBufferLen = 0;


    System.Collections.Generic.List<Delegate> _asyns = new System.Collections.Generic.List<Delegate>();

    public Session(string name)
    {
        _sessionName = name;
        _sendBuffer = new byte[MAX_BUFFER_SIZE];
        _recvBuffer = new byte[MAX_BUFFER_SIZE];
        _sendQue = new System.Collections.Generic.Queue<byte[]>();

    }

    public void OnGetDNS(IAsyncResult result)
    {
        try
        {
            IPAddress[] addrs = Dns.EndGetHostEntry(result).AddressList;
            foreach (var addr in addrs)
            {
                if (_addr == null ||
                    (_addr.AddressFamily != AddressFamily.InterNetworkV6 && addr.AddressFamily == AddressFamily.InterNetworkV6))
                {
                    _addr = addr;
                }
                if (_addr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    break;
                }
            }
            if (_addr == null)
            {
                state = SessionStatus.SS_UNINIT;
                Debug.LogError("Session[" + _sessionName + "]::OnGetDNS can not resolve host. "
                    + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                return;
            }
            state = SessionStatus.SS_INITED;
            Debug.Log("Session[" + _sessionName + "]::OnGetDNS resolve host success. addr=" + _addr.ToString()
               + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
        }
        catch (Exception e)
        {
            state = SessionStatus.SS_UNINIT;
            Debug.LogError("Session[" + _sessionName + "]::OnGetDNS resolve host had except. " 
                + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
        }
    }
    public bool Init(string host, ushort port, string encrypt)
    {
        Debug.Log("Session[" + _sessionName + "]::Init host=" + host + ", port=" + port + ", encrypt=" + encrypt + ", state=" + state);
        _encrypt = encrypt.Trim();
        host = host.Trim();
        if (host.Length == 0 || port == 0)
        {
            Debug.LogError("Session[" + _sessionName + "]::Init error. host param illegality. "
                + "host=" + host + ", port=" + port + ", encrypt=" + encrypt + ", state=" + state);
            return false;
        }
        if (state != SessionStatus.SS_UNINIT)
        {
            Debug.LogError("Session[" + _sessionName + "]::Init error. state not uninit. "
                + "host=" + host + ", port=" + port + ", encrypt=" + encrypt + ", state=" + state);
            return false;
        }
        state = SessionStatus.SS_INITING;
        _addr = null;
        _port = port;

        do
        {
            if (IPAddress.TryParse(host, out _addr))
            {
                if (_addr != null)
                {
                    state = SessionStatus.SS_INITED;
                    _addr = IPAddress.Parse(host);
                    Debug.Log("Session[" + _sessionName + "]::Init parse addr success. addr=" + _addr.ToString() 
                        + " host=" + host + ", port=" + port + ", encrypt=" + encrypt + ", state=" + state);
                    break;
                }
            }
            Debug.Log("Session[" + _sessionName + "]::Init try resolve host."
                + " host=" + host + ", port=" + port + ", encrypt=" + encrypt + ", state=" + state);
            Dns.BeginGetHostEntry(host, new AsyncCallback(OnGetDNS), this);
        }
        while (false);
        return true;
    }

    public void Connect()
    {
        Debug.Log("Session[" + _sessionName + "]::Connect addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
        if (state != SessionStatus.SS_INITED)
        {
            Debug.LogError("Session[" + _sessionName + "]::Connect error. state not inited. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            return;
        }

        _lastConnectTime = Time.realtimeSinceStartup;
        _lastRecvTime = Time.realtimeSinceStartup;

        try
        {
            _rc4Recv = new RC4Encryption();
            _rc4Recv.makeSBox(_encrypt);
            _rc4Send = new RC4Encryption();
            _rc4Send.makeSBox(_encrypt);
            state = SessionStatus.SS_CONNECTING;
            _socket = new Socket(_addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = false;
            _socket.NoDelay = true;
            _socket.BeginConnect(_addr, _port, new AsyncCallback(OnAsyncConnect), _socket);

        }
        catch (Exception e)
        {
            state = SessionStatus.SS_INITED;
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
            Debug.LogError("Session[" + _sessionName + "]::Connect had except. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
        }
    }
    void OnAsyncConnect(IAsyncResult result)
    {
        Debug.Log("Session[" + _sessionName + "]::OnAsyncConnect addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
        try
        {
            Socket socket = result.AsyncState as Socket;
            socket.EndConnect(result);
            if (socket != _socket )
            {
                Debug.LogError("Session[" + _sessionName + "]::Connect error. callback socket not current socket."
                    + " addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                return;
            }
            if (state != SessionStatus.SS_CONNECTING)
            {
                Debug.LogError("Session[" + _sessionName + "]::Connect error. state not connecting. "
                     + " addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                return;
            }

            state = SessionStatus.SS_WORKING;
            _reconnect = 50;

            Debug.Log("Session[" + _sessionName + "]::Connect success. "
                + " addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            if (whenConnected != null)
            {
                PushAsync(whenConnected);
            }
        }
        catch (Exception e)
        {
            PushAsync((System.Action)delegate () { Close(); });
            Debug.LogError("Session[" + _sessionName + "]::Connect had except."
                + " addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e );
        }
    }
    public void Send(byte[] data, string protoName)
    {
        Debug.Log("Session[" + _sessionName + "]::Send<" + protoName + "> data[" + data.Length + "] addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
        try
        {
            if (state != SessionStatus.SS_WORKING)
            {
                Debug.LogError("Session[" + _sessionName + "]::Send<" + protoName + "> data[" + data.Length + "] Faild. status not working. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                return;
            }
            if (_sendBufferLen >0 || _sendQue.Count > 0)
            {
                _sendQue.Enqueue(data);
                Debug.Log("Session[" + _sessionName + "]::Send<" + protoName + "> data[" + data.Length + "] pushed queue. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            }
            else
            {
                data.CopyTo(_sendBuffer, 0);
                if (_encrypt.Length > 0)
                {
                    _rc4Send.encryption(_sendBuffer, 0, data.Length);
                }
                _sendBufferLen = data.Length;

                int ret = _socket.Send(_sendBuffer, 0, _sendBufferLen, SocketFlags.None);
                if (ret == _sendBufferLen)
                {
                    _sendBufferLen = 0;
                }
                else if (ret > 0)
                {
                    Array.Copy(_sendBuffer, ret, _sendBuffer, 0, _sendBufferLen - ret);
                    _sendBufferLen -= ret;
                }
                Debug.Log("Session[" + _sessionName + "]::Send<" + protoName + "> data[" + data.Length + "] direct. sent len=" + ret + ", addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Session[" + _sessionName + "]::Send<" + protoName + "> data[" + data.Length + "] had except. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" +e);
            Close();
        }
    }
    public void  Send<Proto>(Proto proto) where Proto : Proto4z.IProtoObject
    {
        try
        {
            ProtoHeader ph = new ProtoHeader();
            ph.reserve = 0;
            Type pType = proto.GetType();
            var mi = pType.GetMethod("getProtoID");
            if (mi == null)
            {
                Debug.LogError("Session[" + _sessionName + "]::Send<Proto> error. class unsupport. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                return;
            }
            ph.protoID = (ushort)mi.Invoke(proto, null);
            var bin = proto.__encode().ToArray();
            ph.packLen = ProtoHeader.HeadLen + bin.Length;
            var pack = ph.__encode();
            pack.AddRange(bin);
            Send(pack.ToArray(), proto.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("Session[" + _sessionName + "]::Send<Proto> had except. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
        }
    }
    public void Close()
    {
        Debug.Log("Session[" + _sessionName + "]::Close. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", reconnect=" + _reconnect);
        if (state == SessionStatus.SS_CLOSED)
        {
            return;
        }
        if (_reconnect > 0)
        {
            _reconnect--;
        }

        _recvBufferLen = 0;
        _sendBufferLen = 0;
        _sendQue.Clear();
        if (_socket != null)
        {
            _socket.Close();
            _socket = null;
        }
        if (_reconnect > 0 && state != SessionStatus.SS_UNINIT)
        {
            state = SessionStatus.SS_INITED;
        }
        else
        {
            state = SessionStatus.SS_CLOSED;
        }
        Debug.Log("Session[" + _sessionName + "]::Close finish. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", reconnect=" + _reconnect);

    }
    public void PushAsync(Delegate dlg)
    {
        lock(_asyns)
        {
            _asyns.Add(dlg);
        }
    }
    void ProcessAllAsync()
    {
        System.Collections.Generic.List<Delegate> tmp = new System.Collections.Generic.List<Delegate>();
        lock (_asyns)
        {
            tmp.AddRange(_asyns);
            _asyns.Clear();
        }
        foreach (var dlg in tmp)
        {
            try
            {
                dlg.DynamicInvoke(null);
            }
            catch (Exception e)
            {
                Debug.LogError("Session[" + _sessionName + "]::ProcessAllAsync had except. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
            }
        }
    }

    void CheckReconnect()
    {
        try
        {
            if (state == SessionStatus.SS_INITED)
            {
                //两次Connect间隔最短不能小于3秒  
                if (Time.realtimeSinceStartup - _lastConnectTime > 3.0)
                {
                    Connect();
                }
                return;
            }
            if (state == SessionStatus.SS_CONNECTING)
            {
                //Connect超过7秒还没成功就算超时.  
                if (Time.realtimeSinceStartup - _lastConnectTime > 7.0)
                {
                    Close();
                }
                return;
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Session[" + _sessionName + "]::CheckReconnect had except. addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
        }
    }

    void ProcessReceive()
    {
        if (state != SessionStatus.SS_WORKING)
        {
            return;
        }
        if (_recvBufferLen >= MAX_BUFFER_SIZE)
        {
            Debug.LogError("Session[" + _sessionName + "]::ProcessReceive error  _recvBufferLen overflow. _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE 
                + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            Close();
            return;
        }
        try
        {
            do
            {
                
                int total = _socket.Available;
                if (total == 0 && !_socket.Poll(0, SelectMode.SelectRead))
                {
                    break; // 没有可读数据 
                }
                int ret = 0;
                try
                {
                    SocketError se;
                    ret = _socket.Receive(_recvBuffer, _recvBufferLen, MAX_BUFFER_SIZE - _recvBufferLen, SocketFlags.None, out se);
                    if (se != SocketError.Success )
                    {
                        if (se == SocketError.WouldBlock)
                        {
                            return;
                        }
                        Debug.LogError("Session[" + _sessionName + "]::ProcessReceive socket SocketError . _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                             + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + se);
                        Close();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Session[" + _sessionName + "]::ProcessReceive socket remote close.. _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                         + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state +", e=" + e);
                    Close();
                    return;
                }
                if (ret <= 0)
                {
                    Debug.LogError("Session[" + _sessionName + "]::ProcessReceive. impossibility event");
                    Close();
                    return;
                }
               
                if (_encrypt.Length > 0)
                {
                    _rc4Recv.encryption(_recvBuffer, _recvBufferLen, ret);
                }
                _recvBufferLen += ret;
                int offset = 0;

                while (_recvBufferLen - offset >= ProtoHeader.HeadLen)
                {
                    ProtoHeader ph = new ProtoHeader();
                    int headTmp = offset;
                    ph.__decode(_recvBuffer, ref headTmp);
                    if (ph.packLen < ProtoHeader.HeadLen || headTmp != offset + ProtoHeader.HeadLen)
                    {
                        Debug.LogError("Session[" + _sessionName + "]::ProcessReceive. socket __decode wrong. ph.packLen=" + ph.packLen  +", offset=" + offset +", pack offset=" + headTmp
                            + ", _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                            + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
                        Close();
                        return;
                    }
                    if (ph.packLen <= _recvBufferLen - offset)
                    {
                        var pack = new byte[ph.packLen - ProtoHeader.HeadLen];
                        Array.Copy(_recvBuffer, offset + ProtoHeader.HeadLen, pack, 0, ph.packLen - ProtoHeader.HeadLen);
                        try
                        {
                            ProcessPackage(ph.protoID, pack);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Session[" + _sessionName + "]::ProcessReceive. ProcessPackage had error. ph.packLen=" + ph.packLen + ", offset=" + offset 
                                + ", _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                                + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
                        }
                        offset += ph.packLen;
                    }
                    else
                    {
                        break;
                    }
                }
                if (offset > 0)
                {
                    if (_recvBufferLen == offset)
                    {
                        _recvBufferLen = 0;
                    }
                    else
                    {
                        Array.Copy(_recvBuffer, offset, _recvBuffer, 0, _recvBufferLen - offset);
                        _recvBufferLen -= offset;
                    }

                }

            } while (false);
        }

        catch (Exception e)
        {
            Debug.LogError("Session[" + _sessionName + "]::ProcessReceive. had except . ph.packLen="
                + ", _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);

        }
    }

    void ProcessSendRemain()
    {
        if (state != SessionStatus.SS_WORKING)
        {
            return;
        }

        try
        {
            //send 
            if (_sendBufferLen > 0 || _sendQue.Count > 0)
            {
                while (_sendQue.Count > 0)
                {
                    if (_sendQue.Peek().Length <= MAX_BUFFER_SIZE - _sendBufferLen)
                    {
                        var pack = _sendQue.Dequeue();
                        pack.CopyTo(_sendBuffer, _sendBufferLen);
                        if (_encrypt.Length > 0)
                        {
                            _rc4Send.encryption(_sendBuffer, _sendBufferLen, pack.Length);
                        }
                        _sendBufferLen += pack.Length;
                    }
                    else
                    {
                        break;
                    }
                }
                if (_sendBufferLen > 0) // conditional  when invalid pack 
                {
                    int ret = _socket.Send(_sendBuffer, 0, _sendBufferLen, SocketFlags.None);
                    if (ret == _sendBufferLen)
                    {
                        _sendBufferLen = 0;
                    }
                    else if (ret > 0)
                    {
                        Array.Copy(_sendBuffer, ret, _sendBuffer, 0, _sendBufferLen - ret);
                        _sendBufferLen -= ret;
                    }
                }
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Session[" + _sessionName + "]::ProcessSendRemain. had except . ph.packLen="
                + ", _recvBufferLen=" + _recvBufferLen + ", max=" + MAX_BUFFER_SIZE
                + ", addr = " + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state + ", e=" + e);
            Close();
        }
    }

    void ProcessPackage(ushort protoID, byte[] bin)
    {
        string protoName = Proto4z.Reflection.getProtoName(protoID);
        Debug.Log("Session[" + _sessionName + "]::ProcessPackage<" + protoName + "> data[" + bin.Length + "] addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
        _lastRecvTime = Time.realtimeSinceStartup;

        var typeInfo = Type.GetType("Proto4z." + protoName);
        if (typeInfo == null)
        {
            Debug.LogError("Session[" + _sessionName + "]::ProcessPackage<" + protoName + "> data[" + bin.Length + "] error. can not reflect the proto type. "
                + ", addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            return;
        }
            
        var methodInfo = typeInfo.GetMethod("__decode");
        if (methodInfo == null)
        {
            Debug.LogError("Session[" + _sessionName + "]::ProcessPackage<" + protoName + "> data[" + bin.Length + "] error. reflect type hadn't __decode. "
                + ", addr=" + _addr + ", port=" + _port + ", encrypt=" + _encrypt + ", state=" + state);
            return;
        }
        var inst = Activator.CreateInstance(typeInfo);
        int offset = 0;
        methodInfo.Invoke(inst, new object[] { bin, offset });
        Facade.dispatcher.TriggerEvent(protoName, new object[] { inst });
    }

    public void Update()
    {
        ProcessAllAsync();
        if (state == SessionStatus.SS_UNINIT)
        {
            return;
        }
        CheckReconnect();
        if (state != SessionStatus.SS_WORKING)
        {
            return;
        }
        ProcessReceive();
        ProcessSendRemain();
    }
}

