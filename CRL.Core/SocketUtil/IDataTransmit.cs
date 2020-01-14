using System;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// 传输对象接口
    /// </summary>
    public interface IDataTransmit
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// 连接失败事件
        /// </summary>
        event NetEventHandler ConnectFail;
        /// <summary>
        /// 连接成功事件
        /// </summary>
        event NetEventHandler ConnectSucceed;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        event NetEventHandler DisConnected;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        event NetEventHandler ReceiveData;
        /// <summary>
        /// 获取远程终结点
        /// </summary>
        System.Net.EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="bin">二进制数据</param>
        /// <returns></returns>
        bool Send(byte[] bin);
        /// <summary>
        /// 发送数据,并指定发送完成后是否关掉SOCKET
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="sendAndClose"></param>
        /// <returns></returns>
        bool Send(byte[] bin, bool sendAndClose);
        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <returns></returns>
        bool Send(string text);
        /// <summary>
        /// 开始接收数据
        /// </summary>
        void Start();
        /// <summary>
        /// 停止并断开连接
        /// </summary>
        void Stop();
        /// <summary>
        /// Socket对象.
        /// </summary>
        System.Net.Sockets.Socket TcpSocket { get; set;}
    }
}
