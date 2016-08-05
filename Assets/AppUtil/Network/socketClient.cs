using UnityEngine;
using Proto4z;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

enum SessionStatus
{
    SS_UNINIT,
    SS_INITED,
    SS_CONNECTING,
    SS_WORKING,
    SS_CLOSED,
}
class Session
{
    Socket _socket;
    SessionStatus _status = SessionStatus.SS_UNINIT;
    IPAddress _addr;
    ushort _port;
    bool _reconnect = true;
    float _lastConnectTime = 0.0f;
    const int MAX_BUFFER_SIZE = 200 * 1024;
    private byte[] _sendBuffer;
    private int _sendBufferLen = 0;
    private byte[] _recvBuffer;
    private int _recvBufferLen = 0;
    public Session()
    {
        _sendBuffer = new byte[MAX_BUFFER_SIZE];
        _recvBuffer = new byte[MAX_BUFFER_SIZE];
    }
    public bool Init(string host, ushort port)
    {
        try
        {
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
            _status = SessionStatus.SS_INITED;
            _addr = null;
            _port = port;
            IPAddress[] addrs = Dns.GetHostEntry(host).AddressList;
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
                Debug.logger.Log(LogType.Error, "Session::Init can't resolve host. host=" + host + ", port=" + port);
                return false;
            }
        }
        catch (Exception e)
        {
            _status = SessionStatus.SS_UNINIT;
            Debug.logger.Log(LogType.Error, "Session::Init had except. host=" + host + ", port=" + port + ",e=" + e);
            return false;
        }
        return true;
    }
    public void Connect()
    {
        Debug.logger.Log("Session::Connect addr=" + _addr + ", port=" + _port);
        _lastConnectTime = Time.realtimeSinceStartup;
        if (_status != SessionStatus.SS_INITED)
        {
            Debug.logger.Log(LogType.Error, "BeginConnect Session status error. addr=" + _addr + ", port=" + _port +", status =" + _status);
            return;
        }

        try
        {
            _status = SessionStatus.SS_CONNECTING;
            _socket = new Socket(_addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(_addr, _port, new AsyncCallback(onConnect), _socket);
        }
        catch (Exception e)
        {
            _status = SessionStatus.SS_CLOSED;
            Debug.logger.Log(LogType.Error, "Session::Init had except. addr=" + _addr + ", port=" + _port + ",e=" + e);
        }
    }
    public void onConnect(IAsyncResult result)
    {
        try
        {
            Socket socket = result.AsyncState as Socket;
            if(socket != _socket )
            {
                Debug.logger.Log(LogType.Warning, "Session::onConnect _socket not AsyncState. host=" + _addr + ", port=" + _port + ", status =" + _status);
                return;
            }
            if (_status != SessionStatus.SS_CONNECTING)
            {
                Debug.logger.Log(LogType.Warning, "Session::onConnect status error . host=" + _addr + ", port=" + _port + ", status =" + _status);
                return;
            }
            socket.EndConnect(result);
            socket.Blocking = false;
            _status = SessionStatus.SS_WORKING;
        }
        catch (Exception e)
        {
            _status = SessionStatus.SS_CLOSED;
            Debug.logger.Log(LogType.Error, "Session::onConnect had except. host=" + _addr + ", port=" + _port + ",e=" + e);
        }
    }

    public void Close(bool reconnect = false)
    {
        if (_status == SessionStatus.SS_CLOSED)
        {
            return;
        }
        _recvBufferLen = 0;
        _sendBufferLen = 0;
        if(_socket != null)
        {
            _socket.Close();
            _socket = null;
        }
        if (reconnect && _status != SessionStatus.SS_UNINIT)
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
            if (_status == SessionStatus.SS_INITED)
            {
                //两次Connect间隔最短不能小于3秒  
                if (Time.realtimeSinceStartup - _lastConnectTime > 3000)
                {
                    Connect();
                }
                return;
            }
            if (_status == SessionStatus.SS_CONNECTING)
            {
                //Connect超过7秒还没成功就算超时.  
                if (Time.realtimeSinceStartup - _lastConnectTime > 7000)
                {
                    Close(_reconnect);
                }
                return;
            }

            if (_status != SessionStatus.SS_WORKING)
            {
                return;
            }
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
                    _recvBufferLen += ret;
                    //check message 
                } while (true);
            }
            if (_sendBufferLen > 0)
            {

            }
            
        }
        catch (Exception e)
        {
            Debug.logger.Log(LogType.Error, "Session::Update Receive or Send had except. host=" + _addr + ", port=" + _port + ",e=" + e);
            Close();
        }
    }
}

class Client
{
    class NetHeader : IProtoObject
    {
        public uint packLen;
        public ushort reserve;
        public ushort protoID;
        public System.Collections.Generic.List<byte> __encode()
        {
            var ret = new System.Collections.Generic.List<byte>();
            ret.AddRange(BaseProtoObject.encodeUI32(packLen));
            ret.AddRange(BaseProtoObject.encodeUI16(reserve));
            ret.AddRange(BaseProtoObject.encodeUI16(protoID));
            return ret;
        }
        public System.Int32 __decode(byte[] binData, ref System.Int32 pos)
        {
            packLen = BaseProtoObject.decodeUI32(binData, ref pos);
            reserve = BaseProtoObject.decodeUI16(binData, ref pos);
            protoID = BaseProtoObject.decodeUI16(binData, ref pos);
            return pos;
        }
    }


    public void Run()
    {
        RC4Encryption rc4Server = new RC4Encryption();
        RC4Encryption rc4Client = new RC4Encryption();
        rc4Server.makeSBox("zhangyawei");
        rc4Client.makeSBox("zhangyawei");

        IPAddress ip = IPAddress.Parse("127.0.0.1");
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress[] ips = Dns.GetHostEntry("baidu.com").AddressList;
            clientSocket.Connect(new IPEndPoint(ip, 26001));
            Debug.logger.Log("connect Success.");
        }
        catch
        {
            Debug.logger.Log("connect Failed");
            return;
        }
        if(true)
        {
            ClientAuthReq req = new ClientAuthReq("test", "123");
            var binData = req.__encode().ToArray();
            //binData = rc4Client.encryption(binData, binData.Length());

            var sendData = new System.Collections.Generic.List<byte>();

            NetHeader head = new NetHeader();
            head.packLen = (UInt32)(4 + 2 + 2 + binData.Length);
            head.protoID = Proto4z.ClientAuthReq.getProtoID();
            sendData.AddRange(head.__encode());
            sendData.AddRange(binData);
            clientSocket.Send(sendData.ToArray());

            var recvBytes = new byte[2000];
            uint curLen = 0;
            uint needLen = 4 + 2 + 2; //暂时分两段读 后面要改buff接收提高效率 
            uint recvLen = 0;
            NetHeader recvHead = new NetHeader();
            do
            {
                recvLen = (uint)clientSocket.Receive(recvBytes, (int)curLen, (int)needLen, System.Net.Sockets.SocketFlags.None);//第一段 
                if (recvLen == 0)
                {
                    // remote close socket.
                    return;
                }
                curLen += recvLen;
                needLen -= recvLen;
                if (needLen == 0 && curLen == 4 + 2 + 2) ////第一段 完成 
                {
                    int pos = 0;
                    recvHead.__decode(recvBytes, ref pos);
                    needLen = recvHead.packLen - 4 - 2 - 2; //设置第二段 
                }
                else if (needLen == 0) //第二段完成 
                {
                    if (recvHead.protoID == Proto4z.ClientAuthResp.getProtoID())
                    {
                        ClientAuthResp result = new ClientAuthResp();
                        int pos = 4 + 2 + 2;
                        result.__decode(recvBytes, ref pos);

                        Debug.logger.Log("ClientAuthResp: account=" + result.account + ", token=" + result.token + ",retCode=" + result.retCode);
                        int t = 0;
                        t++;
                    }
                    else if(true) //other proto 
                    {

                    }
                    break; //一个协议接收处理完毕后break 
                }
                recvLen = 0; 
            } while (true);


        } 

    }
}


public class socketClient : MonoBehaviour
{

    void Start () {
        Client client = new Client();
        client.Run();
    }
   
	// Update is called once per frame
	void Update () {
	
	}
}
