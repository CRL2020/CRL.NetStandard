using CRL.Core.Remoting;
using Grpc.Core;
using Grpc.Core.Utils;
using System;

namespace CRL.Grpc.Extend
{
    public class GRpcCallInvoker : CallInvoker
    {
        public readonly Func<Channel> Channel;
        Func<Metadata> metadata;
        PollyAttribute _pollyAttribute;
        public GRpcCallInvoker(PollyAttribute pollyAttribute, Func<Channel> channel, Func<Metadata> _metadata)
        {
            _pollyAttribute = pollyAttribute;
            Channel = GrpcPreconditions.CheckNotNull(channel);
            metadata = _metadata;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Calls.BlockingUnaryCall(CreateCall(method, host, options), request);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Calls.AsyncUnaryCall(CreateCall(method, host, options), request);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Calls.AsyncServerStreamingCall(CreateCall(method, host, options), request);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Calls.AsyncClientStreamingCall(CreateCall(method, host, options));
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Calls.AsyncDuplexStreamingCall(CreateCall(method, host, options));
        }

        protected virtual CallInvocationDetails<TRequest, TResponse> CreateCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
            where TRequest : class
            where TResponse : class
        {
            CallOptions options2;
            //重写header
            if (options.Headers != null)
            {
                options2 = options;
            }
            else
            {
                options2 = new CallOptions(metadata(), options.Deadline, options.CancellationToken);
            }
            var methodName = $"{method.ServiceName}_{method.Name}";
            var pollyData = PollyExtension.Invoke(_pollyAttribute, () =>
            {
                var callRes = new CallInvocationDetails<TRequest, TResponse>(Channel.Invoke(), method, host, options2);
                return new PollyExtension.PollyData<CallInvocationDetails<TRequest, TResponse>>() { Data = callRes };
            }, $"{methodName}");
            var response = pollyData.Data;
            if (!string.IsNullOrEmpty(pollyData.Error))
            {
                throw new Exception(pollyData.Error);
            }
            return response;
            //return new CallInvocationDetails<TRequest, TResponse>(Channel.Invoke(), method, host, options2);
        }
    }
}