using CRL.Core.Remoting;
using Grpc.Core;
using Grpc.Core.Utils;
using System;

namespace CRL.Grpc.Extend
{
    public class GRpcCallInvoker : CallInvoker
    {
        GrpcClientOptions _options;
        IGrpcConnect _grpcConnect;
        public GRpcCallInvoker(IGrpcConnect grpcConnect)
        {
            _options = grpcConnect.GetOptions();
            _grpcConnect = grpcConnect;
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
            if (!a)
            {
                _options.MethodPolicies.TryGetValue("", out methodPollyAttr);
            }
            CallOptions options2;
            //重写header
            if (options.Headers != null)
            {
                options2 = options;
            }
            else
            {
                options2 = new CallOptions(_grpcConnect.GetMetadata(), options.Deadline, options.CancellationToken);
            }

            var pollyData = PollyExtension.Invoke(methodPollyAttr, () =>
            {
                var callRes = new CallInvocationDetails<TRequest, TResponse>(_grpcConnect.GetChannel(), method, host, options2);
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