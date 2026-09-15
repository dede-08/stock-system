using Microsoft.AspNetCore.Mvc;
using api_gestion_productos.Services;
using api_gestion_productos.Models;
using Microsoft.AspNetCore.Authorization;

namespace api_gestion_productos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        ///obtiene los productos activos (paginado)
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool desc = false,
            CancellationToken ct = default)
        {
            if (page < 1) return BadRequest(new { message = "page debe ser >= 1" });
            if (pageSize < 1 || pageSize > 100) return BadRequest(new { message = "pageSize debe estar entre 1 y 100" });

            var result = await _productService.GetAllProductsAsync(page, pageSize, sortBy, desc, ct);
            return Ok(result);
        }

        ///obtiene un producto por su ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest(new { message = "id inválido" });
            var product = await _productService.GetProductByIdAsync(id, ct);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        ///crear un nuevo producto
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto productDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var product = await _productService.CreateProductAsync(productDto, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.id }, product);
        }

        ///actualiza un producto existente
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] UpdateProductDto productDto, CancellationToken ct)
        {
            if (id <= 0) return BadRequest(new { message = "id inválido" });
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var product = await _productService.UpdateProductAsync(id, productDto, ct);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        ///elimina un producto (soft delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest(new { message = "id inválido" });
            var result = await _productService.DeleteProductAsync(id, ct);
            if (!result)
                return NotFound(new { message = "Producto no encontrado" });

            return NoContent();
        }

        ///busca productos por categoria (paginado, case-insensitive exacto)
        [HttpGet("category/{category}")]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetByCategory(
            string category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(category) || category.Length > 50)
                return BadRequest(new { message = "Categoría inválida (1-50 caracteres)" });
            if (page < 1) return BadRequest(new { message = "page debe ser >= 1" });
            if (pageSize < 1 || pageSize > 100) return BadRequest(new { message = "pageSize debe estar entre 1 y 100" });

            var products = await _productService.GetProductsByCategoryAsync(category, page, pageSize, ct);
            return Ok(products);
        }

        ///busca productos por termino de busqueda (paginado, ILIKE)
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> Search(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length > 100)
                return BadRequest(new { message = "El término de búsqueda es requerido (1-100 caracteres)" });
            if (page < 1) return BadRequest(new { message = "page debe ser >= 1" });
            if (pageSize < 1 || pageSize > 100) return BadRequest(new { message = "pageSize debe estar entre 1 y 100" });

            var products = await _productService.SearchProductsAsync(q, page, pageSize, ct);
            return Ok(products);
        }

        ///obtiene productos con stock bajo (paginado)
        [HttpGet("low-stock")]
        public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetLowStock(
            [FromQuery] int threshold = 10,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (threshold < 0 || threshold > 100000) return BadRequest(new { message = "threshold debe estar entre 0 y 100000" });
            if (page < 1) return BadRequest(new { message = "page debe ser >= 1" });
            if (pageSize < 1 || pageSize > 100) return BadRequest(new { message = "pageSize debe estar entre 1 y 100" });

            var products = await _productService.GetLowStockProductsAsync(threshold, page, pageSize, ct);
            return Ok(products);
        }
    }
}
