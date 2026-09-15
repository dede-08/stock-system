namespace api_gestion_productos.Models;

public class ProductStatsDto
{
    public int totalProducts { get; set; }
    public int lowStockProducts { get; set; }
    public int outOfStockProducts { get; set; }
    public decimal totalValue { get; set; }
    public decimal averagePrice { get; set; }
}

public class CategoryStatsDto
{
    public string category { get; set; } = "";
    public int count { get; set; }
    public decimal totalValue { get; set; }
    public decimal averagePrice { get; set; }
}
