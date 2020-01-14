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
    /// TCP���ӷ�������,���ܶ�ͻ���TCP����
    /// </summary>
    public class TcpService<T>
         where T : class, IDataTransmit,new()
    {
        #region �¼�����

        /// <summary>
        /// �ͻ��������¼�
        /// </summary>
        public event NetEventHandler Connected;
        /// <summary>
        /// �ͻ��˶Ͽ��¼�
        /// </summary>
        public event NetEventHandler DisConnect;
        #endregion

        #region �ֶ�
        private readonly int maxsockets;            //���ͻ�������
        private int backlog;                        //������������
        private int port;                           //�����˿�
        private TcpListener listener;               //������
        private Dictionary<EndPoint, T> session;    //�������ӵĿͻ���
        private bool active = false;                //�Ƿ��ڻ״̬
        #endregion

        #region ����
        /// <summary>
        /// �Ƿ��ڻ״̬�����Զ����տͻ�������
        /// </summary>
        public bool Active
        {
            get { return this.active; }
        }
        /// <summary>
        /// ��ȡ�����˿ں�
        /// </summary>
        public int ListenPort
        {
            get { return this.port; }
        }
        /// <summary>
        /// ��ȡ��ǰ�ͻ�������
        /// </summary>
        public int ConnectCount
        {
            get { return session.Count; }
        }

        /// <summary>
        /// ��ȡ��ͻ����ӵ�����Socket
        /// </summary>
        public Dictionary<EndPoint, T> Session
        {
            get { return session; }
        }
        #endregion

        #region ���캯��
        /// <summary>
        /// ʹ��ָ���˿ڡ����ͻ���������IP��ַ����ʵ��
        /// </summary>
        /// <param name="port">�����Ķ˿ں�</param>
        /// <param name="maxsockets">���ͻ�������</param>
        /// <param name="ip">IP��ַ</param>
        public TcpService(int port, int maxsockets, string ip)
        {
            if (maxsockets < 1)
            {
                throw new ArgumentOutOfRangeException("maxsockets", "�������������С��1");
            }
            this.port = port;
            this.maxsockets = maxsockets;
            this.listener = new TcpListener(new IPEndPoint(IPAddress.Parse(ip), port));
            this.session = new Dictionary<EndPoint, T>();
        }

        /// <summary>
        /// ʹ��ָ���˿ڹ���ʵ��
        /// </summary>
        /// <param name="port">�����Ķ˿�</param>
        public TcpService(int port)
            : this(port, 1000, "0.0.0.0")
        {
        }
        #endregion

        #region ���÷���
        /// <summary>
        /// ��������������,��ʼ�����ͻ�������
        /// </summary>
        /// <param name="backlog">�������Ӷ��е���󳤶ȡ�</param>
        public void Start(int backlog)
        {
            this.backlog = backlog;
            listener.Start(backlog);
            //�����ͻ����������� 
            listener.BeginAcceptSocket(clientConnect, listener);
            this.active = true;
        }

        /// <summary> 
        /// ��������������,��ʼ�����ͻ�������
        /// </summary> 
        public void Start()
        {
            Start(10);
        }
        /// <summary>
        /// �ر���������
        /// </summary>
        public void Stop()
        {
            listener.Stop();
            this.active = false;
        }
        /// <summary>
        /// �Ͽ����пͻ�������
        /// </summary>
        public void DisConnectAll()
        {
            foreach (KeyValuePair<EndPoint, T> kvp in this.session)
            {
                kvp.Value.DisConnected -= new NetEventHandler(work_DisConnect);
                kvp.Value.Stop();
                //�����ͻ��Ͽ��¼�
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
        /// �ر����������Ͽ����пͻ�������
        /// </summary>
        public void Close()
        {
            this.Stop();
            this.DisConnectAll();
        }

        private void clientConnect(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            //���ܿͻ�������,������Socket
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
                    //���������ͻ����������� 
                    IAsyncResult iar = listener.BeginAcceptSocket(clientConnect, listener);
                }
                else
                {   //�ﵽ������ӿͻ���,��رռ���.
                    listener.Stop();
                    this.active = false;
                }

                //�ͻ������ӳɹ��¼�
                NetEventHandler handler = Connected;
                if (handler != null)
                {
                    handler(work, new NetEventArgs("���ܿͻ�����������"));
                }
                Debug.WriteLine(socketPoint.ToString() + " is Connection...Num" + ConnectCount);
            }
            catch
            {
            }
        }

        //�ͻ��˶Ͽ�����
        private void work_DisConnect(IDataTransmit work, NetEventArgs e)
        {
            EndPoint socketPoint = work.RemoteEndPoint;
            session.Remove(socketPoint);

            //����ѹر�������,���,��������
            if (ConnectCount == maxsockets)
            {
                listener.Start(this.backlog);
                IAsyncResult iar =  listener.BeginAcceptSocket(clientConnect, listener);
                this.active = true;
            }

            //�����ͻ��Ͽ��¼�
            NetEventHandler handler = DisConnect;
            if (handler != null)
            {
                handler(work, e);
            }
            Debug.WriteLine(socketPoint.ToString() + " is OnDisConnected...Num" + ConnectCount);
        }
        #endregion
        
        /// <summary>
        ///  �õ�tcp_keepalive�ṹֵ
        /// </summary>
        /// <param name="onoff">�Ƿ�����Keep-Alive</param>
        /// <param name="keepalivetime">�೤ʱ���ʼ��һ��̽�⣨��λ�����룩</param>
        /// <param name="keepaliveinterval">̽��ʱ��������λ�����룩</param>
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
    /// TCP���ӷ�������,���ܶ�ͻ���TCP����
    /// </summary>
    public class TcpService : TcpService<DataTransmit>
    {
        #region ���캯��
        /// <summary>
        /// ʹ��ָ���˿ڡ����ͻ���������IP��ַ����ʵ��
        /// </summary>
        /// <param name="port">�����Ķ˿ں�</param>
        /// <param name="maxsockets">���ͻ�������</param>
        /// <param name="ip">IP��ַ</param>
        public TcpService(int port, int maxsockets, string ip)
            : base(port, maxsockets, ip)
        {
        }
        /// <summary>
        /// ʹ��ָ���˿ڹ���ʵ��
        /// </summary>
        /// <param name="port">�����Ķ˿�</param>
        public TcpService(int port)
            : base(port, 1000, "0.0.0.0")
        {
        }
        #endregion
    }
}
