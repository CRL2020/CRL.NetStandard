using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ApiProxy
{
    /// <summary>
    /// 表示service特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;set;
        }
        /// <summary>
        /// 发送内容类型
        /// </summary>
        public ContentType ContentType
        {
            get; set;
        }
        /// <summary>
        /// 网关服务前辍
        /// </summary>
        public string GatewayPrefix
        {
            get; set;
        }
    }

    /// <summary>
    /// 表示请求特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class MethodAttribute : Attribute
    {
        /// <summary>
        /// 指定请求路径
        /// </summary>
        public string Path
        {
            get; set;
        }
        /// <summary>
        /// 指定提交方式
        /// </summary>
        public HttpMethod Method
        {
            get; set;
        }
        /// <summary>
        /// 发送内容类型
        /// </summary>
        public ContentType ContentType
        {
            get; set;
        }
        /// <summary>
        /// 返回内容类型
        /// </summary>
        public ContentType ResponseContentType
        {
            get; set;
        }
    }
    public enum HttpMethod
    {
        POST,GET,PUT
    }
    public enum ContentType
    {
        NONE,JSON,XML,FORM
    }
}
