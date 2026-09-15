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
    /// <returns>null si el refresh es inválido, expirado o revocado</returns>
    Task<AuthResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);
    /// <returns>false si el usuario no existe o el rol es inválido</returns>
    Task<bool> PromoteAsync(string email, string role, CancellationToken ct = default);
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
            role = UserRoles.User, // el rol nunca viene del cliente
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return await IssuePairAsync(user, ct);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.email.ToLower() == email && u.isActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.password, user.password))
            return null;

        return await IssuePairAsync(user, ct);
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var hash = _tokens.HashRefreshToken(refreshToken);
        var stored = await _context.RefreshTokens
            .Include(r => r.user)
            .FirstOrDefaultAsync(r => r.tokenHash == hash, ct);

        if (stored is null || !stored.IsActive || stored.user is null || !stored.user.isActive)
            return null;

        // Rotación: revoca el usado y emite un par nuevo.
        stored.revokedAt = DateTime.UtcNow;
        var pair = await IssuePairAsync(stored.user, ct, replacedBy: stored);
        stored.replacedByTokenHash = _tokens.HashRefreshToken(pair.refreshToken);
        await _context.SaveChangesAsync(ct);
        return pair;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        var hash = _tokens.HashRefreshToken(refreshToken);
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.tokenHash == hash, ct);
        if (stored is null || stored.IsRevoked) return;
        stored.revokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        return await _context.Users.AsNoTracking()
            .OrderBy(u => u.id)
            .Select(u => new UserDto
            {
                id = u.id,
                email = u.email,
                fullName = u.name + " " + u.lastname,
                role = u.role,
                isActive = u.isActive,
            })
            .ToListAsync(ct);
    }

    public async Task<bool> PromoteAsync(string email, string role, CancellationToken ct = default)
    {
        if (!UserRoles.IsValid(role)) return false;
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == normalized, ct);
        if (user is null) return false;
        user.role = role;
        user.updatedat = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<AuthResponseDto> IssuePairAsync(User user, CancellationToken ct, RefreshToken? replacedBy = null)
    {
        var fullName = $"{user.name} {user.lastname}".Trim();
        var refresh = _tokens.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            userId = user.id,
            tokenHash = _tokens.HashRefreshToken(refresh),
            expiresAt = DateTime.UtcNow.AddDays(_tokens.GetRefreshExpiryDays()),
        });
        if (replacedBy is null)
            await _context.SaveChangesAsync(ct);
        // Si es rotación, el SaveChanges lo hace el llamador junto al revoke.

        return new AuthResponseDto
        {
            token = _tokens.GenerateToken(user.id, fullName, user.email, user.role),
            refreshToken = refresh,
            email = user.email,
            fullName = fullName,
        };
    }
}
