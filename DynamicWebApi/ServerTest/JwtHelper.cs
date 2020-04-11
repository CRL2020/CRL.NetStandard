using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Linq;
using System.Security.Claims;

namespace ServerTest
{
    public static class JwtHelper
    {
        static string SigningKey = "123456789abcdefghijlllss";
        static string Issuer = "user";
        static string Audience = "everyone";
        public static string WriteToken(Dictionary<string, string> claimDict, DateTime exp)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claimDict.Select(x => new Claim(x.Key, x.Value)),
                expires: exp,
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static Tuple<Dictionary<string, string>, DateTime> ReadToken(string jwt)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
            var tokenValidationParams = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = secretKey,
            };
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = jwtTokenHandler.ValidateToken(jwt, tokenValidationParams, out SecurityToken validated);

            var dict = claimsPrincipal.Claims.ToDictionary(b => b.Type, b => b.Value);
            return new Tuple<Dictionary<string, string>, DateTime>(dict, validated.ValidTo);
        }

        static DateTime GetDateTime(int timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = ((long)timeStamp * 10000000);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }

    }
}
