using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// TcpClient 工具类
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
        /// 构造函数
        /// </summary>
        /// <param name="ip">IP 地址</param>
        /// <param name="port">端口</param>
        /// <param name="tryTimes">重试次数</param>
        /// <param name="longConnection">是否长连接</param>
        public TcpClient(string ip, int port, int tryTimes, bool longConnection)
        {
            this.ip = ip;
            this.port = port;
            this.tryTimes = tryTimes;
            this.longConnection = longConnection;
        }

        /// <summary>
        /// 获取最后的错误信息
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
            //返回数据
            if (size > 0)
            {
                byte[] data = new byte[size];
                Array.Copy(buf, data, size);
                return data;
            }
            return null;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendData">待发送的数据</param>
        /// <returns></returns>
        public bool Send(byte[] sendData)
        {
            if (client == null)
            {
                //连接远程主机
                if (!TryConnect()) return false;
            }

            //发送数据
            bool result = TrySend(sendData);

            //关闭连接
            TryClose();
            return result;
        }
        /// <summary>
        /// 发送数据并接收数据
        /// </summary>
        /// <param name="sendData">待发送的数据</param>
        /// <returns></returns>
        public byte[] SendAndReceive(byte[] sendData)
        {
            if (client == null)
            {
                //连接远程主机
                if (!TryConnect()) return null;
            }

            byte[] data = null;
            //发送数据
            if (TrySend(sendData))
            {
                //接收数据
                data = TryReceive();
            }

            //关闭连接
            TryClose();
            return data;
        }
        /// <summary>
        /// 释放连接
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
