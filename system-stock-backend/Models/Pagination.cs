namespace api_gestion_productos.Models;

/// <summary>Envelope estándar para listados paginados.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> items { get; set; } = new List<T>();
    public int total { get; set; }
    public int page { get; set; }
    public int pageSize { get; set; }
    public int totalPages => pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
}

public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int page, int pageSize)
    {
        if (page < 1) page = DefaultPage;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;
        return (page, pageSize);
    }
}
