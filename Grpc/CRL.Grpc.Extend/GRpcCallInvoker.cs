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
        GrpcClientOptions _options;
        public GRpcCallInvoker(PollyAttribute pollyAttribute, Func<Channel> channel, Func<Metadata> _metadata, GrpcClientOptions options)
        {
            _pollyAttribute = pollyAttribute;
            Channel = GrpcPreconditions.CheckNotNull(channel);
            metadata = _metadata;
            _options = options;
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
            var methodName = $"{method.ServiceName}.{method.Name}";
            var key = methodName.Substring(methodName.IndexOf(".") + 1).ToLower();
            var a = _options.MethodPolicies.TryGetValue(key, out PollyAttribute methodPollyAttr);
            PollyAttribute pa = a ? methodPollyAttr : _pollyAttribute;
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

            var pollyData = PollyExtension.Invoke(pa, () =>
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