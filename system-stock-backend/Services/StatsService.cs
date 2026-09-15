using api_gestion_productos.Data;
using api_gestion_productos.Models;
using Microsoft.EntityFrameworkCore;

namespace api_gestion_productos.Services;

public class StatsService : IStatsService
{
    private readonly AppDbContext _context;

    public StatsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductStatsDto> GetProductStatsAsync(int lowStockThreshold = 10, CancellationToken ct = default)
    {
        var baseQuery = _context.Products.AsNoTracking().Where(p => p.isActive);
        var total = await baseQuery.CountAsync(ct);
        if (total == 0)
            return new ProductStatsDto();

        // 3 conteos + 2 agregados; una sola ronda con Task.WhenAll.
        var lowTask = baseQuery.Where(p => p.stock <= lowStockThreshold).CountAsync(ct);
        var outTask = baseQuery.Where(p => p.stock == 0).CountAsync(ct);
        var sumTask = baseQuery.SumAsync(p => p.price * p.stock, ct);
        var avgTask = baseQuery.AverageAsync(p => p.price, ct);
        await Task.WhenAll(lowTask, outTask, sumTask, avgTask);

        return new ProductStatsDto
        {
            totalProducts = total,
            lowStockProducts = lowTask.Result,
            outOfStockProducts = outTask.Result,
            totalValue = Math.Round(sumTask.Result, 2),
            averagePrice = Math.Round(avgTask.Result, 2),
        };
    }

    public async Task<IReadOnlyList<CategoryStatsDto>> GetProductsByCategoryAsync(CancellationToken ct = default)
    {
        return await _context.Products.AsNoTracking()
            .Where(p => p.isActive)
            .GroupBy(p => p.category)
            .Select(g => new CategoryStatsDto
            {
                category = g.Key,
                count = g.Count(),
                totalValue = Math.Round(g.Sum(p => p.price * p.stock), 2),
                averagePrice = Math.Round(g.Average(p => p.price), 2),
            })
            .OrderByDescending(x => x.count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetMostExpensiveProductsAsync(int limit, CancellationToken ct = default)
    {
        limit = ClampLimit(limit, 5);
        return await TopAsync(q => q.OrderByDescending(p => p.price), limit, ct);
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetHighestStockProductsAsync(int limit, CancellationToken ct = default)
    {
        limit = ClampLimit(limit, 5);
        return await TopAsync(q => q.OrderByDescending(p => p.stock), limit, ct);
    }

    public async Task<IReadOnlyList<ProductResponseDto>> GetRecentProductsAsync(int limit, CancellationToken ct = default)
    {
        limit = ClampLimit(limit, 10);
        return await TopAsync(q => q.OrderByDescending(p => p.createdAt), limit, ct);
    }

    private async Task<IReadOnlyList<ProductResponseDto>> TopAsync(
        Func<IQueryable<Product>, IOrderedQueryable<Product>> order,
        int limit, CancellationToken ct)
    {
        var query = order(_context.Products.AsNoTracking().Where(p => p.isActive));
        return await query.Take(limit)
            .Select(p => new ProductResponseDto
            {
                id = p.id,
                name = p.name,
                description = p.description,
                price = p.price,
                stock = p.stock,
                category = p.category,
                createdAt = p.createdAt,
                updatedAt = p.updatedAt,
                isActive = p.isActive,
            })
            .ToListAsync(ct);
    }

    private static int ClampLimit(int limit, int @default) =>
        limit <= 0 ? @default : Math.Min(limit, 100);
}
