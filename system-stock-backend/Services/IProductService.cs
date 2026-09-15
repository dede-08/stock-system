using api_gestion_productos.Models;

namespace api_gestion_productos.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(int page, int pageSize, string? sortBy = null, bool desc = false, CancellationToken ct = default);
    Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto productDto, CancellationToken ct = default);
    Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto productDto, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken ct = default);
    Task<PagedResult<ProductResponseDto>> GetProductsByCategoryAsync(string category, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<ProductResponseDto>> SearchProductsAsync(string searchTerm, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<ProductResponseDto>> GetLowStockProductsAsync(int threshold, int page, int pageSize, CancellationToken ct = default);
}
