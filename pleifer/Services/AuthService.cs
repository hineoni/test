using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using pleifer.Models;
using Microsoft.IdentityModel.Tokens;

namespace pleifer.Services;

public class AuthService
{
    public const string Issuer = "App";
    public const string Audience = "AppClient";
    private const string Key = "12345678901234567890123456789012";

    public string Authenticate(User user)
    {
        var jwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: GenerateClaims(user),
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(
                GetKey(),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public static SymmetricSecurityKey GetKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }

    private static List<Claim> GenerateClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        return claims;
    }

    public bool IsUserAuthenticated(HttpContext context)
    {
        return context.User.Identity!.IsAuthenticated;
    }

    public int GetUserId(HttpContext context)
    {
        return Convert.ToInt32(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}
