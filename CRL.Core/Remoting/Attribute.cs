using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    /// <summary>
    /// 登录切入点
    /// </summary>
    public class LoginPointAttribute: Attribute
    {
    }
    /// <summary>
    /// 不限制访问
    /// </summary>
    public class AllowAnonymousAttribute : Attribute
    {
    }
    public class PollyAttribute : Attribute
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }
        /// <summary>
        /// 重试间隔
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// 多少次后熔断
        /// </summary>
        public int CircuitBreakerCount { get; set; }
        /// <summary>
        /// 熔断时间
        /// </summary>
        public TimeSpan CircuitBreakerTime { get; set; } = TimeSpan.FromSeconds(5);
        /// <summary>
        /// 判定超时时间
        /// </summary>
        public TimeSpan TimeOutTime { get; set; } = TimeSpan.Zero;
    }
}
