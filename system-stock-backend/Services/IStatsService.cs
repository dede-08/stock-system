using api_gestion_productos.Models;

namespace api_gestion_productos.Services;

public interface IStatsService
{
    Task<ProductStatsDto> GetProductStatsAsync(int lowStockThreshold = 10, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryStatsDto>> GetProductsByCategoryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponseDto>> GetMostExpensiveProductsAsync(int limit, CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponseDto>> GetHighestStockProductsAsync(int limit, CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponseDto>> GetRecentProductsAsync(int limit, CancellationToken ct = default);
}
