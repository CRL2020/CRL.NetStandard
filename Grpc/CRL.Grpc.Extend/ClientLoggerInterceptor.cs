using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Grpc.Extend
{
    /// <summary>
    /// gRPC 拦截器
    /// </summary>
    public class ClientLoggerInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);

            return continuation(request, context);
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method)
            where TRequest : class
            where TResponse : class
        {
            var initialColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Starting call. Type: {method.Type}. Request: {typeof(TRequest)}. Response: {typeof(TResponse)}");
            Console.ForegroundColor = initialColor;
        }
    }
}
