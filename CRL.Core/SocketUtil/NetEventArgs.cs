using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// ����ͨѶ�¼�ģ��ί��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">TcpClient</param>
    public delegate void NetEventHandler(IDataTransmit sender, NetEventArgs e);

    /// <summary>
    /// �����¼�����
    /// </summary>
    public class NetEventArgs : EventArgs
    {
        private object eventArg;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="EventArg"></param>
        public NetEventArgs(object EventArg)
        {
            eventArg = EventArg;
        }
        /// <summary>
        /// �¼�����
        /// </summary>
        public object EventArg
        {
            get { return eventArg; }
            set { eventArg = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (eventArg != null)
            {
                return eventArg.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
