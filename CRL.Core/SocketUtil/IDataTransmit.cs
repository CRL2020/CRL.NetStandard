using System;

namespace CRL.Core.SocketUtil
{
    /// <summary>
    /// �������ӿ�
    /// </summary>
    public interface IDataTransmit
    {
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// ����ʧ���¼�
        /// </summary>
        event NetEventHandler ConnectFail;
        /// <summary>
        /// ���ӳɹ��¼�
        /// </summary>
        event NetEventHandler ConnectSucceed;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        event NetEventHandler DisConnected;
        /// <summary>
        /// ���յ������¼�
        /// </summary>
        event NetEventHandler ReceiveData;
        /// <summary>
        /// ��ȡԶ���ս��
        /// </summary>
        System.Net.EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// ���Ͷ���������
        /// </summary>
        /// <param name="bin">����������</param>
        /// <returns></returns>
        bool Send(byte[] bin);
        /// <summary>
        /// ��������,��ָ��������ɺ��Ƿ�ص�SOCKET
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="sendAndClose"></param>
        /// <returns></returns>
        bool Send(byte[] bin, bool sendAndClose);
        /// <summary>
        /// �����ı�
        /// </summary>
        /// <param name="text">�ı�����</param>
        /// <returns></returns>
        bool Send(string text);
        /// <summary>
        /// ��ʼ��������
        /// </summary>
        void Start();
        /// <summary>
        /// ֹͣ���Ͽ�����
        /// </summary>
        void Stop();
        /// <summary>
        /// Socket����.
        /// </summary>
        System.Net.Sockets.Socket TcpSocket { get; set;}
    }
}
