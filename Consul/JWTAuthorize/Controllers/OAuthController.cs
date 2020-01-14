using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JWTAuthorize.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class OAuthController : Controller
    {
        private ITokenHelper tokenHelper = null;
        public OAuthController(ITokenHelper _tokenHelper)
        {
            tokenHelper = _tokenHelper;

        }
        [HttpPost]
        public IActionResult Login([FromBody]User user)
        {
            return Ok(tokenHelper.CreateToken(user));
        }
        [HttpPost]
        [Authorize]
        public IActionResult RefreshToken()
        {
            return Ok(tokenHelper.RefreshToken(Request.HttpContext.User));
        }
    }
}