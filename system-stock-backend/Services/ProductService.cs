using api_gestion_productos.Data;
using api_gestion_productos.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace api_gestion_productos.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(
        int page, int pageSize, string? sortBy = null, bool desc = false, CancellationToken ct = default)
    {
        (page, pageSize) = Pagination.Normalize(page, pageSize);

        var query = _context.Products.AsNoTracking().Where(p => p.isActive);

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "name" => desc ? query.OrderByDescending(p => p.name) : query.OrderBy(p => p.name),
            "price" => desc ? query.OrderByDescending(p => p.price) : query.OrderBy(p => p.price),
            "stock" => desc ? query.OrderByDescending(p => p.stock) : query.OrderBy(p => p.stock),
            "createdat" => desc ? query.OrderByDescending(p => p.createdAt) : query.OrderBy(p => p.createdAt),
            _ => desc ? query.OrderByDescending(p => p.id) : query.OrderBy(p => p.id),
        };

        var total = await query.CountAsync(ct);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProductResponseDto>
        {
            items = _mapper.Map<List<ProductResponseDto>>(entities),
            total = total,
            page = page,
            pageSize = pageSize,
        };
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.id == id && p.isActive, ct);

        return product is null ? null : _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto productDto, CancellationToken ct = default)
    {
        var product = _mapper.Map<Product>(productDto);
        product.createdAt = DateTime.UtcNow;
        product.isActive = true;

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto productDto, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct);
        if (product == null || !product.isActive)
            return null;

        _mapper.Map(productDto, product);
        product.updatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct);
        if (product == null || !product.isActive)
            return false;

        product.isActive = false;
        product.updatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<PagedResult<ProductResponseDto>> GetProductsByCategoryAsync(
        string category, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Pagination.Normalize(page, pageSize);
        var cat = category.Trim();

        var query = _context.Products.AsNoTracking()
            .Where(p => p.isActive && EF.Functions.ILike(p.category, cat))
            .OrderBy(p => p.id);

        var total = await query.CountAsync(ct);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProductResponseDto>
        {
            items = _mapper.Map<List<ProductResponseDto>>(entities),
            total = total,
            page = page,
            pageSize = pageSize,
        };
    }

    public async Task<PagedResult<ProductResponseDto>> SearchProductsAsync(
        string searchTerm, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Pagination.Normalize(page, pageSize);
        var pattern = $"%{EscapeLike(searchTerm.Trim())}%";

        var query = _context.Products.AsNoTracking()
            .Where(p => p.isActive && (
                EF.Functions.ILike(p.name, pattern) ||
                EF.Functions.ILike(p.description, pattern) ||
                EF.Functions.ILike(p.category, pattern)))
            .OrderBy(p => p.id);

        var total = await query.CountAsync(ct);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProductResponseDto>
        {
            items = _mapper.Map<List<ProductResponseDto>>(entities),
            total = total,
            page = page,
            pageSize = pageSize,
        };
    }

    public async Task<PagedResult<ProductResponseDto>> GetLowStockProductsAsync(
        int threshold, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Pagination.Normalize(page, pageSize);

        var query = _context.Products.AsNoTracking()
            .Where(p => p.isActive && p.stock <= threshold)
            .OrderBy(p => p.stock)
            .ThenBy(p => p.id);

        var total = await query.CountAsync(ct);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProductResponseDto>
        {
            items = _mapper.Map<List<ProductResponseDto>>(entities),
            total = total,
            page = page,
            pageSize = pageSize,
        };
    }

    private static string EscapeLike(string input) =>
        input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
