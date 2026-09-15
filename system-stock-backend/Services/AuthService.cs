using api_gestion_productos.Data;
using api_gestion_productos.Models;
using Microsoft.EntityFrameworkCore;

namespace api_gestion_productos.Services;

public interface IAuthService
{
    /// <returns>null si el email ya existe</returns>
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    /// <returns>null si credenciales inválidas</returns>
    Task<AuthResponseDto?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokens;

    public AuthService(AppDbContext context, ITokenService tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var email = dto.email.Trim().ToLowerInvariant();
        var exists = await _context.Users.AnyAsync(u => u.email.ToLower() == email, ct);
        if (exists) return null;

        var user = new User
        {
            name = dto.name.Trim(),
            lastname = dto.lastname.Trim(),
            age = dto.age,
            email = email,
            telephone = dto.telephone?.Trim() ?? "",
            password = BCrypt.Net.BCrypt.HashPassword(dto.password),
            createdat = DateTime.UtcNow,
            isActive = true,
            role = "USER", // el rol nunca viene del cliente
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        var fullName = $"{user.name} {user.lastname}".Trim();
        return new AuthResponseDto
        {
            token = _tokens.GenerateToken(user.id, fullName, user.email, user.role),
            email = user.email,
            fullName = fullName,
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.email.ToLower() == email && u.isActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.password, user.password))
            return null;

        var fullName = $"{user.name} {user.lastname}".Trim();
        return new AuthResponseDto
        {
            token = _tokens.GenerateToken(user.id, fullName, user.email, user.role),
            email = user.email,
            fullName = fullName,
        };
    }
}
