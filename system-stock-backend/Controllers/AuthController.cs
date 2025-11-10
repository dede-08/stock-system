using System.Security.Claims;
using System.Text;
using api_gestion_productos.Data;
using api_gestion_productos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using LoginRequest = api_gestion_productos.Models.LoginRequest;
using DotNetEnv;
using BCrypt.Net;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    
    private readonly AppDbContext _context;
    private readonly string _jwtKey = Env.GetString("API_KEY");

    public AuthController(AppDbContext context)
    {
        _context = context;
        Env.Load();
    }

    [HttpPost("add-user")]
    public IActionResult Register(User user)
    {
        if (_context.Users.Any(u => u.email == user.email))
        {
            return BadRequest("El email ya está registrado.");
        }

        user.password = BCrypt.Net.BCrypt.HashPassword(user.password);
        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok("Usuario registrado correctamente");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.email == request.email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.password, user.password))
        {
            return Unauthorized("Credenciales incorrectas");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.name + " " + user.lastname),
                new Claim(ClaimTypes.Email, user.email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { token = tokenHandler.WriteToken(token) });
    }
}