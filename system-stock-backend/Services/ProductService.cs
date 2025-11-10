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

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.isActive)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.id == id && p.isActive);
        
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        product.createdAt = DateTime.UtcNow;
        product.isActive = true;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.isActive)
            return null;

        _mapper.Map(productDto, product);
        product.updatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.isActive)
            return false;

        product.isActive = false;
        product.updatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryAsync(string category)
    {
        var products = await _context.Products
            .Where(p => p.category.ToLower() == category.ToLower() && p.isActive)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
    }

    public async Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _context.Products
            .Where(p => p.isActive && 
                       (p.name.ToLower().Contains(searchTerm.ToLower()) ||
                        p.description.ToLower().Contains(searchTerm.ToLower()) ||
                        p.category.ToLower().Contains(searchTerm.ToLower())))
            .ToListAsync();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await _context.Products
            .Where(p => p.stock <= threshold && p.isActive)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
    }
}
