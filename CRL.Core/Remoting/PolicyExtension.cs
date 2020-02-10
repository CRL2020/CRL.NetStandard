using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public static class PollyExtension
    {
        public class PollyData<T>
        {
            public T Data;
            public string Error;
        }
        static System.Collections.Concurrent.ConcurrentDictionary<string, object> policies = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();
        public static PollyData<T> Invoke<T>(PollyAttribute atr, Func<PollyData<T>> task,string policyKey)
        {
            if (atr == null)
            {
                return task();
            }
            Policy<PollyData<T>> policy;
            var a = policies.TryGetValue(policyKey,out object _policy);
            if (_policy == null)
            {
                 policy = Policy.NoOp<PollyData<T>>();
                if (atr.CircuitBreakerCount > 0)//熔断
                {
                    policy = policy.Wrap(Policy.Handle<Exception>().CircuitBreaker(atr.CircuitBreakerCount, atr.CircuitBreakerTime));
                    var policyFallBack = Policy<PollyData<T>>.Handle<Polly.CircuitBreaker.BrokenCircuitException>()
          .Fallback(() =>
              {
                  return new PollyData<T>() { Error = "接口调用被熔断" };
              });
                    policy = policyFallBack.Wrap(policy);
                }
                if (atr.TimeOutTime > TimeSpan.Zero)//超时
                {
                    policy = policy.Wrap(Policy.Timeout(() => atr.TimeOutTime, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    var policyFallBack = Policy<PollyData<T>>.Handle<Polly.Timeout.TimeoutRejectedException>()
                        .Fallback(() =>
                        {
                            return new PollyData<T>() { Error = "接口调用超时" };
                        });
                    policy = policyFallBack.Wrap(policy);
                }
                if (atr.RetryCount > 0)//重试
                {
                    policy = policy.Wrap(Policy.Handle<Exception>().WaitAndRetry(atr.RetryCount, b => atr.RetryInterval));
                }
                policies.TryAdd(policyKey, policy);
            }
            else
            {
                policy = _policy as Policy<PollyData<T>>;
            }
            try
            {
                return policy.Execute(task);
            }
            catch(Exception ero)
            {
                var msg = ero.Message;
                if (ero is Request.RequestException)
                {
                    msg = (ero as Request.RequestException).ToString();
                }
                return new PollyData<T>() { Error = $"接口调用发生错误:{msg}" };
            }
        }

        public static async Task<PollyData<T>> InvokeAsync<T>(PollyAttribute atr,  Func<Task<PollyData<T>>> task, string policyKey)
        {
            if (atr == null)
            {
                return await task();
            }
            Policy<PollyData<T>> policy;
            var a = policies.TryGetValue(policyKey, out object _policy);
            if (_policy == null)
            {
                policy = Policy.NoOpAsync<PollyData<T>>();
                if (atr.CircuitBreakerCount > 0)//熔断
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(atr.CircuitBreakerCount, atr.CircuitBreakerTime));
                    var policyFallBack = Policy<PollyData<T>>
                        .Handle<Polly.CircuitBreaker.BrokenCircuitException>()
          .FallbackAsync((ctx, t) =>
          {
              return Task.FromResult(new PollyData<T>() { Error = "接口调用被熔断" });
          }, (ex, t) =>
          {
              return Task.FromResult<PollyData<T>>(null);
          });
                    policy = policyFallBack.WrapAsync(policy);
                }
                if (atr.TimeOutTime > TimeSpan.Zero)//超时
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(() => atr.TimeOutTime, Polly.Timeout.TimeoutStrategy.Pessimistic));
                    var policyFallBack = Policy<PollyData<T>>
                        .Handle<Polly.Timeout.TimeoutRejectedException>()
          .FallbackAsync((ctx, t) =>
          {
              return Task.FromResult(new PollyData<T>() { Error = "接口调用超时" });
          }, (ex, t) =>
          {
              return Task.FromResult<PollyData<T>>(null);
          });
                    policy = policyFallBack.WrapAsync(policy);
                }
                if (atr.RetryCount > 0)//重试
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(atr.RetryCount, b => atr.RetryInterval));
                }
                policies.TryAdd(policyKey, policy);
            }
            else
            {
                policy = _policy as Policy<PollyData<T>>;
            }
            try
            {
                return await policy.ExecuteAsync(task);
            }
            catch (Exception ero)
            {
                var msg = ero.Message;
                if (ero is Request.RequestException)
                {
                    msg = (ero as Request.RequestException).ToString();
                }
                return new PollyData<T>() { Error = $"接口调用发生错误:{msg}" };
            }
        }

    }
}
