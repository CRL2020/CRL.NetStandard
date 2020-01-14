using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRL.Ocelot
{
    public class JwtAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        IConfiguration _configuration;
        public JwtAuthorizeMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var useJwtAuthorize = _configuration.GetValue<bool>("UseJwtAuthorize");
            if (useJwtAuthorize)
            {
                var result = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                var claimsPrincipal = result.Principal;
                if (claimsPrincipal == null)
                {
                    httpContext.Response.StatusCode = 401;
                    return;
                }
            }
            await _next(httpContext);
        }
    }
}
