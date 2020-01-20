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
        public static string WriteToken(Dictionary<string, string> claimDict, DateTime exp)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Instance.Jwt.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: AppSettings.Instance.Jwt.Issuer,
                audience: AppSettings.Instance.Jwt.Audience,
                claims: claimDict.Select(x => new Claim(x.Key, x.Value)),
                expires: exp,
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static (Dictionary<string, string>, DateTime) ReadToken(string jwt)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (token.Claims != null)
            {
                foreach (var claim in token.Claims)
                {
                    dict.Add(claim.Type, claim.Value);
                }
            }
            return (dict, token.ValidTo);
        }
    }
}
