namespace api_gestion_productos.Models;

/// <summary>
/// Refresh token con rotación: solo se guarda el hash SHA256, nunca el token plano.
/// </summary>
public class RefreshToken
{
    public int id { get; set; }
    public int userId { get; set; }
    public User? user { get; set; }
    public string tokenHash { get; set; } = "";
    public DateTime expiresAt { get; set; }
    public DateTime createdAt { get; set; } = DateTime.UtcNow;
    public DateTime? revokedAt { get; set; }
    public string? replacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= expiresAt;
    public bool IsRevoked => revokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
}

public static class UserRoles
{
    public const string User = "USER";
    public const string Admin = "ADMIN";

    public static bool IsValid(string? role) =>
        role == User || role == Admin;
}
