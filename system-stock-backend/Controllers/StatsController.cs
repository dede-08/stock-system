using Microsoft.AspNetCore.Mvc;
using api_gestion_productos.Models;
using api_gestion_productos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;

namespace api_gestion_productos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _stats;

        public StatsController(IStatsService stats)
        {
            _stats = stats;
        }

        ///obtiene las estadisticas generales de los productos
        [HttpGet("products")]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<ProductStatsDto>> GetProductStats(
            [FromQuery] int lowStockThreshold = 10, CancellationToken ct = default)
        {
            if (lowStockThreshold < 0 || lowStockThreshold > 100000)
                return BadRequest(new { message = "lowStockThreshold debe estar entre 0 y 100000" });
            return Ok(await _stats.GetProductStatsAsync(lowStockThreshold, ct));
        }

        ///obtiene productos por categoria con conteo
        [HttpGet("products/by-category")]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<IReadOnlyList<CategoryStatsDto>>> GetProductsByCategory(CancellationToken ct)
        {
            return Ok(await _stats.GetProductsByCategoryAsync(ct));
        }

        ///obtiene los productos con mayor precio
        [HttpGet("products/most-expensive")]
        public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetMostExpensiveProducts(
            [FromQuery] int limit = 5, CancellationToken ct = default)
        {
            if (limit < 1 || limit > 100) return BadRequest(new { message = "limit debe estar entre 1 y 100" });
            return Ok(await _stats.GetMostExpensiveProductsAsync(limit, ct));
        }

        ///obtiene los productos con mayor stock
        [HttpGet("products/highest-stock")]
        public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetHighestStockProducts(
            [FromQuery] int limit = 5, CancellationToken ct = default)
        {
            if (limit < 1 || limit > 100) return BadRequest(new { message = "limit debe estar entre 1 y 100" });
            return Ok(await _stats.GetHighestStockProductsAsync(limit, ct));
        }

        ///obtiene los productos recientemente agregados
        [HttpGet("products/recent")]
        public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetRecentProducts(
            [FromQuery] int limit = 10, CancellationToken ct = default)
        {
            if (limit < 1 || limit > 100) return BadRequest(new { message = "limit debe estar entre 1 y 100" });
            return Ok(await _stats.GetRecentProductsAsync(limit, ct));
        }
    }
}
