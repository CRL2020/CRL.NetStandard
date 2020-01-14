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
    /// �����������
    /// </summary>
    public class DataTransmit : IDataTransmit
    {
        #region �¼�����
        /// <summary>
        /// ���ӳɹ��¼�
        /// </summary>
        public event NetEventHandler ConnectSucceed;
        /// <summary>
        /// ����ʧ���¼�
        /// </summary>
        public event NetEventHandler ConnectFail;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        public event NetEventHandler DisConnected;
        /// <summary>
        /// ���յ������¼�
        /// </summary>
        public event NetEventHandler ReceiveData;
        #endregion

        #region �ֶ�
        private Socket socket;                  //���ӵ�Socket
        private EndPoint iep;                   //�����սڵ�,���ڱ�ʶ��ͬ���û�
        private byte[] buffer;                  //�������ݻ���
        private SocketError errorCode;          //�������
        /// <summary>
        /// �����С
        /// </summary>
        public const int BagSize = 8192;        //�����С
        #endregion

        #region ����
        /// <summary>
        /// ��ȡ������ Socket ����
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
        /// ��ȡԶ���ս��
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return iep; }
        }
        /// <summary>
        /// Socket�Ƿ�������
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
        /// Socket�������
        /// </summary>
        public SocketError ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }
        /// <summary>
        /// ������ɺ��Ƿ�ϵ�
        /// </summary>
        bool SendAndClose;
        #endregion

        #region ���캯��
        /// <summary>
        /// Ĭ�Ϲ��캯��
        /// </summary>
        public DataTransmit()
        {
            errorCode = SocketError.Success;
            buffer = new byte[BagSize];
        }
        /// <summary>
        /// ʹ��ָ����IP��ַ�Ͷ˿ڹ���ʵ��
        /// </summary>
        /// <param name="ip">IP��ַ</param>
        /// <param name="port">�˿�</param>
        public DataTransmit(string ip, int port)
            : this(new IPEndPoint(IPAddress.Parse(ip), port))
        {
        }

        /// <summary>
        /// �ͻ��˵��ô˹��캯��
        /// </summary>
        /// <param name="ipEndPoint">�����ӵķ������������ַ</param>
        public DataTransmit(EndPoint ipEndPoint)
            : this()
        {
            iep = ipEndPoint;
        }

        /// <summary>
        /// �������˵���
        /// </summary>
        /// <param name="client">�������������ӵõ���Socket����</param>
        public DataTransmit(Socket client)
            : this()
        {
            TcpSocket = client;
        }
        #endregion

        /// <summary>
        /// ֹͣ���䣬�Ͽ�����
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
        /// ��ʼ��������
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
            //���ӳɹ�,��ʼ��������
            OnConnectSucceed();
            receiveData();
        }

        private void receiveData()
        {
            // �����첽���� BeginReceive ����֪ socket ��ν�������
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
        /// �������ݻص�����
        /// </summary>
        /// <param name="ar"></param>
        private void receiveCallback(IAsyncResult ar)
        {
            if (socket == null || !socket.Connected) return;

            //���յ������ݳ��ȣ�
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
        /// �����ı�
        /// </summary>
        /// <param name="text">�ı�����</param>
        /// <returns></returns>
        public virtual bool Send(string text)
        {
            byte[] bin = Encoding.Default.GetBytes(text);
            return Send(bin);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Send(byte[] data)
        {
            return Send(data, false);
        }
        /// <summary>
        /// ���Ͳ�ָ����ɺ��Ƿ�ص�SOCKET
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

        #region �ܱ������¼�������

        /// <summary>
        /// �������ӳɹ��¼�
        /// </summary>
        protected virtual void OnConnectSucceed()
        {
            NetEventHandler hander = ConnectSucceed;
            if (hander != null)
            {
                ConnectSucceed(this, new NetEventArgs("�ɹ����ӵ�������"));
            }
        }

        /// <summary>
        /// ��������ʧ���¼�
        /// </summary>
        /// <param name="err"></param>
        protected virtual void OnConnectFail(Exception err)
        {
            NetEventHandler hander = ConnectFail;   //���ӷ�����ʧ���¼�
            if (hander != null)
            {
                ConnectFail(this, new NetEventArgs(err));
            }
        }

        /// <summary>
        /// �������ӶϿ��¼�
        /// </summary>
        /// <param name="err"></param>
        protected virtual void OnDisConnected(Exception err)
        {
            //Stop();
            NetEventHandler hander = DisConnected;  //�Ͽ������¼�
            if (hander != null)
            {
                hander(this, new NetEventArgs(err));
            }
        }

        /// <summary>
        /// �������������¼�
        /// </summary>
        /// <param name="bin"></param>
        protected virtual void OnReceiveData(object bin)
        {
            NetEventHandler hander = ReceiveData;   //���յ���Ϣ�¼�
            if (hander != null)
            {
                hander(this, new NetEventArgs(bin));
            }
        }
        #endregion
    }
}
