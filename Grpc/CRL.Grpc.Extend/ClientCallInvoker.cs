using CRL.Core.Remoting;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace CRL.Grpc.Extend
{
    internal sealed class ClientCallInvoker : CallInvoker
    {
        PollyAttribute _pollyAttribute;
        GRpcCallInvoker _callInvoke;
        Interceptor _interceptor;
        public ClientCallInvoker(PollyAttribute pollyAttribute, GRpcCallInvoker callInvoker, Interceptor interceptor)
        {
            _interceptor = interceptor;
            _pollyAttribute = pollyAttribute;
            _callInvoke = callInvoker;
        }

        private TResponse Call<TResponse>(Func<CallInvoker, TResponse> call, string methodName)
        {
            var channel = _callInvoke.Channel;
            var pollyData = PollyExtension.Invoke(_pollyAttribute, () =>
            {
                var callRes = call(_callInvoke);
                return new PollyExtension.PollyData<TResponse>() { Data = callRes };
            }, $"{methodName}");
            var response = pollyData.Data;
            if (!string.IsNullOrEmpty(pollyData.Error))
            {
                throw new Exception(pollyData.Error);
            }
            return response;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            //var _context = new ClientInterceptorContext<TRequest, TResponse>(method, host, options);
            return Call(ci => ci.BlockingUnaryCall(method, host, options, request), $"{method.ServiceName}_{method.Name}");
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Call(ci => ci.AsyncUnaryCall(method, host, options, request), $"{method.ServiceName}_{method.Name}");
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Call(ci => ci.AsyncServerStreamingCall(method, host, options, request), $"{method.ServiceName}_{method.Name}");
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Call(ci => ci.AsyncClientStreamingCall(method, host, options), $"{method.ServiceName}_{method.Name}");
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Call(ci => ci.AsyncDuplexStreamingCall(method, host, options), $"{method.ServiceName}_{method.Name}");
        }
    }
}