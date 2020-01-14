using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// TcpClient ������
    /// </summary>
    public class TcpClient
    {
        private string ip;
        private int port;
        private int tryTimes;
        private bool longConnection;
        private System.Net.Sockets.TcpClient client;
        private Exception lastException;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="ip">IP ��ַ</param>
        /// <param name="port">�˿�</param>
        /// <param name="tryTimes">���Դ���</param>
        /// <param name="longConnection">�Ƿ�����</param>
        public TcpClient(string ip, int port, int tryTimes, bool longConnection)
        {
            this.ip = ip;
            this.port = port;
            this.tryTimes = tryTimes;
            this.longConnection = longConnection;
        }

        /// <summary>
        /// ��ȡ���Ĵ�����Ϣ
        /// </summary>
        public Exception LastException
        {
            get { return this.lastException; }
        }

        private bool TryConnect()
        {
            for (int i = 0; i < tryTimes; i++)
            {
                try
                {
                    client = new System.Net.Sockets.TcpClient(this.ip, this.port);
                    return true;
                }
                catch (Exception ex)
                {
                    this.lastException = ex;
                }
            }
            return false;
        }
        private void TryClose()
        {
            if (!longConnection)
            {
                if (client.Client != null)
                    client.Client.Close();
                if (client != null)
                    client.Close();
                client = null;
            }
        }
        private bool TrySend(byte[] data)
        {
            for (int i = 0; i < tryTimes; i++)
            {
                try
                {
                    client.Client.Send(data);
                    return true;
                }
                catch (Exception ex)
                {
                    this.TryConnect();
                    this.lastException = ex;
                }
            }
            return false;
        }
        private byte[] TryReceive()
        {
            byte[] buf = new byte[8192];
            int size = 0;
            for (int i = 0; i < tryTimes; i++)
            {
                try
                {
                    size = client.Client.Receive(buf);
                    break;
                }
                catch (Exception ex)
                {
                    this.lastException = ex;
                }
            }
            //��������
            if (size > 0)
            {
                byte[] data = new byte[size];
                Array.Copy(buf, data, size);
                return data;
            }
            return null;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sendData">�����͵�����</param>
        /// <returns></returns>
        public bool Send(byte[] sendData)
        {
            if (client == null)
            {
                //����Զ������
                if (!TryConnect()) return false;
            }

            //��������
            bool result = TrySend(sendData);

            //�ر�����
            TryClose();
            return result;
        }
        /// <summary>
        /// �������ݲ���������
        /// </summary>
        /// <param name="sendData">�����͵�����</param>
        /// <returns></returns>
        public byte[] SendAndReceive(byte[] sendData)
        {
            if (client == null)
            {
                //����Զ������
                if (!TryConnect()) return null;
            }

            byte[] data = null;
            //��������
            if (TrySend(sendData))
            {
                //��������
                data = TryReceive();
            }

            //�ر�����
            TryClose();
            return data;
        }
        /// <summary>
        /// �ͷ�����
        /// </summary>
        public void Dispose()
        {
            if (client.Client != null)
                client.Client.Close();
            if (client != null)
                client.Close();
            client = null;
        }
    }
}
