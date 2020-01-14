using System;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CRL.Core.SocketUtil
{

    /// <summary>
    /// TCP连接服务器端,接受多客户的TCP连接
    /// </summary>
    public class TcpService<T>
         where T : class, IDataTransmit,new()
    {
        #region 事件定义

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event NetEventHandler Connected;
        /// <summary>
        /// 客户端断开事件
        /// </summary>
        public event NetEventHandler DisConnect;
        #endregion

        #region 字段
        private readonly int maxsockets;            //最大客户连接数
        private int backlog;                        //最大挂起连接数
        private int port;                           //监听端口
        private TcpListener listener;               //监听类
        private Dictionary<EndPoint, T> session;    //保存连接的客户端
        private bool active = false;                //是否处于活动状态
        #endregion

        #region 属性
        /// <summary>
        /// 是否处于活动状态，即自动接收客户端连接
        /// </summary>
        public bool Active
        {
            get { return this.active; }
        }
        /// <summary>
        /// 获取监听端口号
        /// </summary>
        public int ListenPort
        {
            get { return this.port; }
        }
        /// <summary>
        /// 获取当前客户连接数
        /// </summary>
        public int ConnectCount
        {
            get { return session.Count; }
        }

        /// <summary>
        /// 获取与客户连接的所有Socket
        /// </summary>
        public Dictionary<EndPoint, T> Session
        {
            get { return session; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 使用指定端口、最大客户连接数、IP地址构造实例
        /// </summary>
        /// <param name="port">监听的端口号</param>
        /// <param name="maxsockets">最大客户连接量</param>
        /// <param name="ip">IP地址</param>
        public TcpService(int port, int maxsockets, string ip)
        {
            if (maxsockets < 1)
            {
                throw new ArgumentOutOfRangeException("maxsockets", "最大连接数不能小于1");
            }
            this.port = port;
            this.maxsockets = maxsockets;
            this.listener = new TcpListener(new IPEndPoint(IPAddress.Parse(ip), port));
            this.session = new Dictionary<EndPoint, T>();
        }

        /// <summary>
        /// 使用指定端口构造实例
        /// </summary>
        /// <param name="port">监听的端口</param>
        public TcpService(int port)
            : this(port, 1000, "0.0.0.0")
        {
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 启动服务器程序,开始监听客户端请求
        /// </summary>
        /// <param name="backlog">挂起连接队列的最大长度。</param>
        public void Start(int backlog)
        {
            this.backlog = backlog;
            listener.Start(backlog);
            //监听客户端连接请求 
            listener.BeginAcceptSocket(clientConnect, listener);
            this.active = true;
        }

        /// <summary> 
        /// 启动服务器程序,开始监听客户端请求
        /// </summary> 
        public void Start()
        {
            Start(10);
        }
        /// <summary>
        /// 关闭侦听器。
        /// </summary>
        public void Stop()
        {
            listener.Stop();
            this.active = false;
        }
        /// <summary>
        /// 断开所有客户端连接
        /// </summary>
        public void DisConnectAll()
        {
            foreach (KeyValuePair<EndPoint, T> kvp in this.session)
            {
                kvp.Value.DisConnected -= new NetEventHandler(work_DisConnect);
                kvp.Value.Stop();
                //触发客户断开事件
                NetEventHandler handler = DisConnect;
                if (handler != null)
                {
                    handler(kvp.Value, new NetEventArgs(new SocketException((int)SocketError.Success)));
                }
            }
            this.session.Clear();
        }
        public void Log(string msg)
        {
            CRL.Core.EventLog.Log(this.GetType() + "\t" + msg, "socket", false);
        }
        /// <summary>
        /// 关闭侦听器并断开所有客户端连接
        /// </summary>
        public void Close()
        {
            this.Stop();
            this.DisConnectAll();
        }

        private void clientConnect(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            //接受客户的连接,得连接Socket
            try
            {
                Socket client = listener.EndAcceptSocket(ar);
                client.IOControl(IOControlCode.KeepAliveValues, Keepalive(0, 30000, 10000), null);

                T work = new T();
                work.TcpSocket = client;
                work.DisConnected += new NetEventHandler(work_DisConnect);

                EndPoint socketPoint = client.RemoteEndPoint;
                if (session.ContainsKey(socketPoint))
                {
                    session[socketPoint] = work;
                }
                else
                {
                    session.Add(socketPoint, work);
                }

                if (ConnectCount < maxsockets)
                {
                    //继续监听客户端连接请求 
                    IAsyncResult iar = listener.BeginAcceptSocket(clientConnect, listener);
                }
                else
                {   //达到最大连接客户数,则关闭监听.
                    listener.Stop();
                    this.active = false;
                }

                //客户端连接成功事件
                NetEventHandler handler = Connected;
                if (handler != null)
                {
                    handler(work, new NetEventArgs("接受客户的连接请求"));
                }
                Debug.WriteLine(socketPoint.ToString() + " is Connection...Num" + ConnectCount);
            }
            catch
            {
            }
        }

        //客户端断开连接
        private void work_DisConnect(IDataTransmit work, NetEventArgs e)
        {
            EndPoint socketPoint = work.RemoteEndPoint;
            session.Remove(socketPoint);

            //如果已关闭侦听器,则打开,继续监听
            if (ConnectCount == maxsockets)
            {
                listener.Start(this.backlog);
                IAsyncResult iar =  listener.BeginAcceptSocket(clientConnect, listener);
                this.active = true;
            }

            //触发客户断开事件
            NetEventHandler handler = DisConnect;
            if (handler != null)
            {
                handler(work, e);
            }
            Debug.WriteLine(socketPoint.ToString() + " is OnDisConnected...Num" + ConnectCount);
        }
        #endregion
        
        /// <summary>
        ///  得到tcp_keepalive结构值
        /// </summary>
        /// <param name="onoff">是否启用Keep-Alive</param>
        /// <param name="keepalivetime">多长时间后开始第一次探测（单位：毫秒）</param>
        /// <param name="keepaliveinterval">探测时间间隔（单位：毫秒）</param>
        /// <returns></returns>
        public static byte[] Keepalive(int onoff, int keepalivetime, int keepaliveinterval)
        {
            byte[] inOptionValues = new byte[12];
            BitConverter.GetBytes(onoff).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes(keepalivetime).CopyTo(inOptionValues, 4);
            BitConverter.GetBytes(keepaliveinterval).CopyTo(inOptionValues, 8);
            return inOptionValues;
        }
    }

    /// <summary>
    /// TCP连接服务器端,接受多客户的TCP连接
    /// </summary>
    public class TcpService : TcpService<DataTransmit>
    {
        #region 构造函数
        /// <summary>
        /// 使用指定端口、最大客户连接数、IP地址构造实例
        /// </summary>
        /// <param name="port">监听的端口号</param>
        /// <param name="maxsockets">最大客户连接量</param>
        /// <param name="ip">IP地址</param>
        public TcpService(int port, int maxsockets, string ip)
            : base(port, maxsockets, ip)
        {
        }
        /// <summary>
        /// 使用指定端口构造实例
        /// </summary>
        /// <param name="port">监听的端口</param>
        public TcpService(int port)
            : base(port, 1000, "0.0.0.0")
        {
        }
        #endregion
    }
}
