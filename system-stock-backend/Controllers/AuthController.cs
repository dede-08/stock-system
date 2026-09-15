using System.Security.Claims;
using api_gestion_productos.Models;
using api_gestion_productos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace api_gestion_productos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("add-user")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _auth.RegisterAsync(dto, ct);
        if (result is null)
            return Conflict(new { message = "El email ya está registrado." });

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _auth.LoginAsync(request, ct);
        if (result is null)
            return Unauthorized(new { message = "Credenciales incorrectas" });

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.refreshToken))
            return BadRequest(new { message = "refreshToken requerido" });

        var result = await _auth.RefreshAsync(dto.refreshToken, ct);
        if (result is null)
            return Unauthorized(new { message = "Refresh token inválido o expirado" });

        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(dto.refreshToken))
            await _auth.LogoutAsync(dto.refreshToken, ct);
        return NoContent();
    }

    [HttpGet("users")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken ct)
        => Ok(await _auth.GetUsersAsync(ct));

    [HttpPost("promote")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Promote([FromBody] PromoteUserDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.email) || !UserRoles.IsValid(dto.role))
            return BadRequest(new { message = "email y role (USER|ADMIN) requeridos" });

        var ok = await _auth.PromoteAsync(dto.email, dto.role, ct);
        if (!ok) return NotFound(new { message = "Usuario no encontrado" });
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
        => Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            fullName = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role),
        });
}
