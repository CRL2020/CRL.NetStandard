using System.Security.Claims;

namespace JWTAuthorize
{
    public interface ITokenHelper
    {
        ComplexToken CreateToken(User user);
        ComplexToken CreateToken(Claim[] claims);
        Token RefreshToken(ClaimsPrincipal claimsPrincipal);

    }
}
