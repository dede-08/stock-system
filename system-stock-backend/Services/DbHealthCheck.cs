using api_gestion_productos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace api_gestion_productos.Services;

/// <summary>Health check sin dependencias extra: SELECT 1 a Postgres.</summary>
public class DbHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;
    public DbHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return HealthCheckResult.Healthy("postgres ok");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("postgres down", ex);
        }
    }
}
