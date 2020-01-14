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
}
