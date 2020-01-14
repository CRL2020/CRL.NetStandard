using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Text;

namespace CRL.RPC
{
    class ClientHandler : ChannelHandlerAdapter
    {
        ResponseWaits waits { get; }
        public ClientHandler(ResponseWaits _waits)
        {
            waits = _waits;
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            //Console.WriteLine(context.Channel.Id);
            var buffer = message as IByteBuffer;
            var response = ResponseMessage.FromBuffer(buffer);
            waits.Set(response.MsgId, response);
        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("ExceptionCaught: " + exception);
            context.CloseAsync();
        }
    }
}