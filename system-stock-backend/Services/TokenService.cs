using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace api_gestion_productos.Services;

public interface ITokenService
{
    string GenerateToken(int userId, string fullName, string email, string role);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    int GetRefreshExpiryDays();
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(int userId, string fullName, string email, string role)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException(
            "JWT key no configurada. Define JWT_KEY / Jwt:Key (ver .env.example).");
        if (key.Length < 32)
            throw new InvalidOperationException("JWT key debe tener al menos 32 caracteres.");

        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expiryMinutes = _config.GetValue<int?>("Jwt:ExpiryMinutes") ?? 120;

        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role ?? "USER"),
            }),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    /// <summary>Token opaco de 256 bits en Base64Url (43 chars). Único por llamada.</summary>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    /// <summary>SHA256 hex del refresh token. Es lo único que se persiste.</summary>
    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public int GetRefreshExpiryDays()
    {
        var days = _config.GetValue<int?>("Jwt:RefreshExpiryDays") ?? 7;
        return days < 1 ? 7 : Math.Min(days, 30);
    }
}
