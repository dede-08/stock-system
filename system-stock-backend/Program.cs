using System.Text;
using System.Threading.RateLimiting;
using api_gestion_productos.Data;
using api_gestion_productos.Middleware;
using api_gestion_productos.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// .env solo para desarrollo local. En prod usar variables de entorno / KeyVault.
// Debe cargarse ANTES de crear el builder para que IConfiguration las vea.
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Serilog como único provider (sin doble consola). Lee sección "Serilog".
builder.Logging.ClearProviders();
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOutputCache();
builder.Services.AddHealthChecks().AddCheck<DbHealthCheck>("postgres");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' no configurada. " +
            "Define ConnectionStrings__DefaultConnection (ver .env.example).");
    }
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 3);
        npgsql.CommandTimeout(15);
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con documentación
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gestión de inventarios para tiendas",
        Version = "v1",
        Description = "API para gestión de inventarios con autenticación JWT",
        Contact = new OpenApiContact
        {
            Name = "Ben André"
        }
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// registrar servicios
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(typeof(Program));

// configurar CORS desde config (no hardcodeado)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate limiting: frena brute-force en login/register
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// JWT config desde IConfiguration (Jwt:Key/Issuer/Audience)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT key no configurada o muy corta (mín. 32 chars). " +
        "Define JWT_KEY / Jwt:Key (ver .env.example) y rota la clave expuesta.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(2),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
    };
});

var app = builder.Build();

// Bootstrap: el email en Admin:Email (env ADMIN_EMAIL) queda como ADMIN.
// Best-effort: si la DB no existe aún (pre-migración), solo se loguea.
using (var scope = app.Services.CreateScope())
{
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminBootstrap");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var adminEmail = app.Configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var admin = await db.Users.FirstOrDefaultAsync(u => u.email.ToLower() == adminEmail);
            if (admin is not null && admin.role != "ADMIN")
            {
                admin.role = "ADMIN";
                await db.SaveChangesAsync();
                log.LogInformation("Usuario {email} promovido a ADMIN por bootstrap.", adminEmail);
            }
        }
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Admin bootstrap omitido (DB no disponible?).");
    }
}

// Middleware de errores PRIMERO para atrapar todo.
app.UseExceptionHandling();
app.UseCorrelationId();
app.UseSecurityHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión de Productos v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// usar CORS
app.UseCors("Frontend");

app.UseRateLimiter();

app.UseOutputCache();

// usar middleware de logging
app.UseRequestLogging();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
