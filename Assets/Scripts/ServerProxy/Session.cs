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
    SessionStatus _status = SessionStatus.SS_UNINIT;
    public SessionStatus  Status{get{return _status;}}
    public Action _onConnect;

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




    System.Collections.Generic.Queue<Delegate> _asyns = new System.Collections.Generic.Queue<Delegate>();
    public Session()
    {
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
                _status = SessionStatus.SS_UNINIT;
                Debug.logger.Log(LogType.Error, "Session::OnGetDNS can't resolve host.");
                return;
            }
            _status = SessionStatus.SS_INITED;
            Debug.logger.Log(LogType.Log, "Session::OnGetDNS resolve dns=" + _addr);
        }
        catch (Exception e)
        {
            _status = SessionStatus.SS_UNINIT;
            Debug.logger.Log(LogType.Error, "Session::OnGetDNS had except. e=" + e);
        }
    }
    public bool Init(string host, ushort port, string encrypt)
    {
        _encrypt = encrypt.Trim();
        host = host.Trim();
        if (host.Length == 0 || port == 0)
        {
            Debug.logger.Log(LogType.Error, "Session::Init Session param error. host=" + host + ", port=" + port + ", status=" + _status);
            return false;
        }
        if (_status != SessionStatus.SS_UNINIT)
        {
            Debug.logger.Log(LogType.Error, "Session::Init Session status error. host=" + host + ", port=" + port + ", status=" + _status);
            return false;
        }
        _status = SessionStatus.SS_INITING;
        _addr = null;
        _port = port;

        do
        {
            if (IPAddress.TryParse(host, out _addr))
            {
                if (_addr != null)
                {
                    _status = SessionStatus.SS_INITED;
                    _addr = IPAddress.Parse(host);
                    break;
                }
            }
            Dns.BeginGetHostEntry(host, new AsyncCallback(OnGetDNS), this);
        }
        while (false);
        return true;
    }
    public void Connect()
    {
        Debug.logger.Log("Session::Connect addr=" + _addr + ", port=" + _port);
        _lastConnectTime = Time.realtimeSinceStartup;
        _lastRecvTime = Time.realtimeSinceStartup;
        if (_status != SessionStatus.SS_INITED)
        {
            Debug.logger.Log(LogType.Error, "BeginConnect Session status error. addr=" + _addr + ", port=" + _port +", status =" + _status);
            return;
        }

        try
        {
            _rc4Recv = new RC4Encryption();
            _rc4Recv.makeSBox(_encrypt);
            _rc4Send = new RC4Encryption();
            _rc4Send.makeSBox(_encrypt);
            _status = SessionStatus.SS_CONNECTING;
            _socket = new Socket(_addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = false;
            _socket.BeginConnect(_addr, _port, new AsyncCallback(OnConnect), _socket);

        }
        catch (Exception e)
        {
            _status = SessionStatus.SS_INITED;
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
            Debug.logger.Log(LogType.Error, "Session::Init had except. addr=" + _addr + ", port=" + _port + ",e=" + e);
        }
    }
    public void OnConnect(IAsyncResult result)
    {
        try
        {
            Socket socket = result.AsyncState as Socket;
            socket.EndConnect(result);
            if (socket != _socket )
            {
                Debug.logger.Log(LogType.Warning, "Session::onConnect _socket not AsyncState. host=" + _addr + ", port=" + _port + ", status =" + _status);
                return;
            }
            if (_status != SessionStatus.SS_CONNECTING)
            {
                Debug.logger.Log(LogType.Warning, "Session::onConnect status error . host=" + _addr + ", port=" + _port + ", status =" + _status);
                return;
            }
            Debug.Log("Session::onConnect connected. host=" + _addr + ", port=" + _port );
            _status = SessionStatus.SS_WORKING;
            _reconnect = 50;
            if (_onConnect != null)
            {
                _asyns.Enqueue(_onConnect);
            }
        }
        catch (Exception e)
        {
            Debug.logger.Log(LogType.Error, "Session::onConnect had except. host=" + _addr + ", port=" + _port + ",e=" + e);
            _asyns.Enqueue((System.Action)delegate () { Close(); });
        }
    }
    public void Send(byte[] data)
    {
        _sendQue.Enqueue(data);
    }
    public void  Send<Proto>(Proto proto) where Proto : Proto4z.IProtoObject
    {
        ProtoHeader ph = new ProtoHeader();
        ph.reserve = 0;
        Type pType = proto.GetType();
        var mi = pType.GetMethod("getProtoID");
        if (mi == null)
        {
            Debug.logger.Log(LogType.Error, "Session::Send can not find method getProtoID. ");
            return;
        }
        ph.protoID = (ushort)mi.Invoke(proto, null);
        var bin = proto.__encode().ToArray();
        ph.packLen = ProtoHeader.HeadLen + bin.Length;
        var pack = ph.__encode();
        pack.AddRange(bin);
        Send(pack.ToArray());
    }


    public void OnRecv(ushort protoID, byte[] bin)
    {
        string protoName = Proto4z.Reflection.getProtoName(protoID);
        Debug.logger.Log("recv pack len=" + bin.Length + ", protoID=" + protoID + ", protoName=" + protoName);
        _lastRecvTime = Time.realtimeSinceStartup;
        try
        {
            var typeInfo = Type.GetType("Proto4z." + protoName);
            if (typeInfo == null)
            {
                Debug.logger.Log(LogType.Error, "not found reflection type info. len=" + bin.Length + ", protoID=" + protoID + ", protoName=" + protoName);
                return;
            }
            
            var methodInfo = typeInfo.GetMethod("__decode");
            if (methodInfo == null)
            {
                Debug.logger.Log(LogType.Error, "not found reflection method info. len=" + bin.Length + ", protoID=" + protoID + ", protoName=" + protoName);
                return;
            }
            var inst = Activator.CreateInstance(typeInfo);
            int offset = 0;
            methodInfo.Invoke(inst, new object[] { bin, offset });
            Facade._dispatcher.TriggerEvent(protoName, new object[] { inst });
        }
        catch (Exception)
        {
            Debug.logger.Log(LogType.Error, "exception. len=" + bin.Length + ", protoID=" + protoID + ", protoName=" + protoName);
        }


    }
    public void Close()
    {
        if (_status == SessionStatus.SS_CLOSED)
        {
            return;
        }
        if (_reconnect > 0 )
        {
            _reconnect--;
        }
        
        _recvBufferLen = 0;
        _sendBufferLen = 0;
        _sendQue.Clear();
        if(_socket != null)
        {
            _socket.Close();
            _socket = null;
        }
        if (_reconnect > 0 && _status != SessionStatus.SS_UNINIT)
        {
            _status = SessionStatus.SS_INITED;
        }
        else
        {
            _status = SessionStatus.SS_CLOSED;
        }
    }
    public void Update()
    {
        try
        {
            while (_asyns.Count > 0)
            {
                var dele = _asyns.Dequeue();
                dele.DynamicInvoke(null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Session run Async queue error. e=" + e);
        }

        try
        {
            if (_status == SessionStatus.SS_UNINIT)
            {
                return;
            }
            //Debug.logger.Log("cur=" + Time.realtimeSinceStartup + ", last=" + _lastConnectTime);
            if (_status == SessionStatus.SS_INITED)
            {
                //两次Connect间隔最短不能小于3秒  
                if (Time.realtimeSinceStartup - _lastConnectTime > 3.0)
                {
                    Connect();
                }
                return;
            }
            if (_status == SessionStatus.SS_CONNECTING)
            {
                //Connect超过7秒还没成功就算超时.  
                if (Time.realtimeSinceStartup - _lastConnectTime > 7.0)
                {
                    Close();
                }
                return;
            }

            if (_status != SessionStatus.SS_WORKING)
            {
                return;
            }


        }
        catch (Exception e)
        {
            Debug.LogError("Session run Async queue error. e=" + e);
        }

        try
        {
            //Receive 每帧只读取一次, 每次都尽可能去读满缓冲.  
            if (_recvBufferLen < MAX_BUFFER_SIZE)
            {
                do
                {
                    int total = _socket.Available;
                    if (total == 0)
                    {
                        break; // 没有可读数据 
                    }
                    int ret = _socket.Receive(_recvBuffer, _recvBufferLen, MAX_BUFFER_SIZE - _recvBufferLen, SocketFlags.None);
                    if (ret <= 0)
                    {
                        Debug.logger.Log(LogType.Error, "!!!Unintended!!! remote closed socket. host=" + _addr + ", port=" + _port + ", status =" + _status);
                        Close();
                        return;
                    }
                    if (_encrypt.Length > 0)
                    {
                        _rc4Recv.encryption(_recvBuffer, _recvBufferLen, ret);
                    }
                    _recvBufferLen += ret;
                    //check message 
                    int offset = 0;
                    while (_recvBufferLen - offset >= ProtoHeader.HeadLen)
                    {
                        ProtoHeader ph = new ProtoHeader();
                        ph.__decode(_recvBuffer, ref offset);
                        if (ph.packLen <= _recvBufferLen - (offset - ProtoHeader.HeadLen))
                        {
                            var pack = new byte[ph.packLen - ProtoHeader.HeadLen];
                            Array.Copy(_recvBuffer, offset, pack, 0,  ph.packLen - ProtoHeader.HeadLen);
                            OnRecv(ph.protoID, pack);
                            offset += (ph.packLen - ProtoHeader.HeadLen);
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
                    
                } while (true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Session run Async queue error. e=" + e);
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
            Debug.logger.Log(LogType.Error, "Session::Update Receive or Send had except. host=" + _addr + ", port=" + _port + ",e=" + e);
            Close();
        }
    }
}

