using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// 网络通讯事件模型委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">TcpClient</param>
    public delegate void NetEventHandler(IDataTransmit sender, NetEventArgs e);

    /// <summary>
    /// 网络事件参数
    /// </summary>
    public class NetEventArgs : EventArgs
    {
        private object eventArg;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="EventArg"></param>
        public NetEventArgs(object EventArg)
        {
            eventArg = EventArg;
        }
        /// <summary>
        /// 事件参数
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
