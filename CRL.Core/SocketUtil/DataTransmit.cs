using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// 辅助传输对象
    /// </summary>
    public class DataTransmit : IDataTransmit
    {
        #region 事件定义
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event NetEventHandler ConnectSucceed;
        /// <summary>
        /// 连接失败事件
        /// </summary>
        public event NetEventHandler ConnectFail;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event NetEventHandler DisConnected;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event NetEventHandler ReceiveData;
        #endregion

        #region 字段
        private Socket socket;                  //连接的Socket
        private EndPoint iep;                   //网络终节点,用于标识不同的用户
        private byte[] buffer;                  //接收数据缓存
        private SocketError errorCode;          //错误代码
        /// <summary>
        /// 缓存大小
        /// </summary>
        public const int BagSize = 8192;        //缓存大小
        #endregion

        #region 属性
        /// <summary>
        /// 获取或设置 Socket 对象
        /// </summary>
        public Socket TcpSocket
        {
            get
            {
                return socket;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("client");
                }
                this.socket = value;
                this.socket.ReceiveBufferSize = BagSize;
                this.iep = value.RemoteEndPoint;
            }
        }
        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return iep; }
        }
        /// <summary>
        /// Socket是否已连接
        /// </summary>
        public bool Connected
        {
            get
            {
                if (socket == null)
                {
                    return false;
                }
                else
                {
                    return this.socket.Connected;
                }
            }
        }
        /// <summary>
        /// Socket错误代码
        /// </summary>
        public SocketError ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }
        /// <summary>
        /// 发送完成后是否断掉
        /// </summary>
        bool SendAndClose;
        #endregion

        #region 构造函数
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DataTransmit()
        {
            errorCode = SocketError.Success;
            buffer = new byte[BagSize];
        }
        /// <summary>
        /// 使用指定的IP地址和端口构造实例
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public DataTransmit(string ip, int port)
            : this(new IPEndPoint(IPAddress.Parse(ip), port))
        {
        }

        /// <summary>
        /// 客户端调用此构造函数
        /// </summary>
        /// <param name="ipEndPoint">在连接的服务器端网络地址</param>
        public DataTransmit(EndPoint ipEndPoint)
            : this()
        {
            iep = ipEndPoint;
        }

        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <param name="client">服务器监听连接得到的Socket对象</param>
        public DataTransmit(Socket client)
            : this()
        {
            TcpSocket = client;
        }
        #endregion

        /// <summary>
        /// 停止传输，断开连接
        /// </summary>
        public void Stop()
        {
            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Disconnect(false);
                        OnDisConnected(new SocketException((int)SocketError.Success));
                    }
                    socket.Close();
                }
                catch { }
                socket = null;
            }
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            if (socket != null && socket.Connected)
            {
                receiveData();
            }
            else
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.ReceiveBufferSize = BagSize;
                this.socket.BeginConnect(iep, connectCallback, socket);
            }
        }

        private void connectCallback(IAsyncResult ar)
        {
            try
            {
                this.socket.EndConnect(ar);
            }
            catch (Exception err)
            {
                OnConnectFail(err);
                return;
            }
            //连接成功,开始接收数据
            OnConnectSucceed();
            receiveData();
        }

        private void receiveData()
        {
            // 调用异步方法 BeginReceive 来告知 socket 如何接收数据
            if (socket != null && socket.Connected)
            {
                IAsyncResult iar = socket.BeginReceive(buffer, 0, BagSize, SocketFlags.None, out errorCode, receiveCallback, buffer);
                if ((errorCode != SocketError.Success) && (errorCode != SocketError.IOPending))
                {
                    OnDisConnected(new SocketException((int)errorCode));
                }
            }
        }

        /// <summary>
        /// 接收数据回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void receiveCallback(IAsyncResult ar)
        {
            if (socket == null || !socket.Connected) return;

            //接收到的数据长度．
            int receLen = 0;
            try
            {
                receLen = socket.EndReceive(ar, out errorCode);
            }
            catch (Exception err)
            {
                OnDisConnected(err);
                return;
            }
            if (errorCode == SocketError.Success)
            {
                if (receLen > 0)
                {
                    byte[] currentBin = new byte[receLen];
                    Buffer.BlockCopy(buffer, 0, currentBin, 0, receLen);
                    OnReceiveData(currentBin);
                    receiveData();
                }
                else
                {
                    OnDisConnected(new SocketException((int)SocketError.NotConnected));
                }
            }
            else
            {
                OnDisConnected(new SocketException((int)errorCode));
            }
        }

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <returns></returns>
        public virtual bool Send(string text)
        {
            byte[] bin = Encoding.Default.GetBytes(text);
            return Send(bin);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Send(byte[] data)
        {
            return Send(data, false);
        }
        /// <summary>
        /// 发送并指定完成后是否关掉SOCKET
        /// </summary>
        public virtual bool Send(byte[] data,bool sendAndClose)
        {
            SendAndClose = sendAndClose;
            if (Connected)
            {
                this.socket.BeginSend(data, 0, data.Length, SocketFlags.None, out errorCode, sendCallBack, socket);
                if (errorCode == SocketError.Success)
                {
                    return true;
                }
            }
            return false;
        }

        private void sendCallBack(IAsyncResult ar)
        {
            if (socket == null) return;
            try
            {
                this.socket.EndSend(ar, out errorCode);
                if (SendAndClose)
                {
                    Stop();
                }
            }
            catch (Exception err)
            {
                OnDisConnected(err);
                return;
            }
            if (errorCode != SocketError.Success)
            {
                OnDisConnected(new SocketException((int)errorCode));
            }
        }

        #region 受保护的事件处理方法

        /// <summary>
        /// 触发连接成功事件
        /// </summary>
        protected virtual void OnConnectSucceed()
        {
            NetEventHandler hander = ConnectSucceed;
            if (hander != null)
            {
                ConnectSucceed(this, new NetEventArgs("成功连接到服务器"));
            }
        }

        /// <summary>
        /// 触发连接失败事件
        /// </summary>
        /// <param name="err"></param>
        protected virtual void OnConnectFail(Exception err)
        {
            NetEventHandler hander = ConnectFail;   //连接服务器失败事件
            if (hander != null)
            {
                ConnectFail(this, new NetEventArgs(err));
            }
        }

        /// <summary>
        /// 触发连接断开事件
        /// </summary>
        /// <param name="err"></param>
        protected virtual void OnDisConnected(Exception err)
        {
            //Stop();
            NetEventHandler hander = DisConnected;  //断开连接事件
            if (hander != null)
            {
                hander(this, new NetEventArgs(err));
            }
        }

        /// <summary>
        /// 触发接收数据事件
        /// </summary>
        /// <param name="bin"></param>
        protected virtual void OnReceiveData(object bin)
        {
            NetEventHandler hander = ReceiveData;   //接收到消息事件
            if (hander != null)
            {
                hander(this, new NetEventArgs(bin));
            }
        }
        #endregion
    }
}
