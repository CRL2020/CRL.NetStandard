using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.SocketUtil
{
    public class ListenTest : TcpService
    {
        public ListenTest(int port):base(port)
        {
            Connected += new NetEventHandler(server_Connected);
            DisConnect += new NetEventHandler(server_DisConnect);
        }
        
        void server_DisConnect(IDataTransmit sender, NetEventArgs e)
        {
            Log(sender.RemoteEndPoint.ToString() + " 连接断开");
        }

        void server_Connected(IDataTransmit sender, NetEventArgs e)
        {
            Log(sender.RemoteEndPoint.ToString() + " 连接成功");
            sender.ReceiveData += new NetEventHandler(sender_ReceiveData);
            //接收数据
            sender.Start();
        }

        //接收数据并修改部分数据然后发送回去
        void sender_ReceiveData(IDataTransmit sender, NetEventArgs e)
        {
            try
            {
                byte[] data = (byte[])e.EventArg;
                
                //发送数据
                sender.Send(data);
                Log(sender.RemoteEndPoint.ToString() + " 发送数据");
            }
            catch (Exception ex)
            {
                Log("处理数据出错：" + ex.Message);
            }
        }
    }
}
