using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace JWTApiTest
{
    /// <summary>
    /// customer permission policy handler
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<JwtAuthorizationRequirement>
    {
        /// <summary>
        /// authentication scheme provider
        /// </summary>
        readonly IAuthenticationSchemeProvider _schemes;
        IHttpContextAccessor _IHttpContextAccessor;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="schemes"></param>
        public PermissionHandler(IAuthenticationSchemeProvider schemes, IHttpContextAccessor IHttpContextAccessor)
        {
            _IHttpContextAccessor = IHttpContextAccessor;
               _schemes = schemes;
        }
        /// <summary>
        /// handle requirement
        /// </summary>
        /// <param name="context">authorization handler context</param>
        /// <param name="jwtAuthorizationRequirement">jwt authorization requirement</param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, JwtAuthorizationRequirement jwtAuthorizationRequirement)
        {
            //see https://q.cnblogs.com/q/120091/
            var httpContext = _IHttpContextAccessor.HttpContext;
            var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
            var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);

            var claimsPrincipal = result.Principal;
            if (claimsPrincipal == null)
            {
                context.Fail();
                return;
            }
            var routeEndpoint = context.Resource as RouteEndpoint;

            var a = jwtAuthorizationRequirement.ValidatePermission(routeEndpoint, claimsPrincipal);
            if (a)
            {
                context.Succeed(jwtAuthorizationRequirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
