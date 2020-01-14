using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthorize
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public int AccessTokenExpiresMinutes { get; set; }

        public string RefreshTokenAudience { get; set; }
        public int RefreshTokenExpiresMinutes { get; set; }
    }
}
