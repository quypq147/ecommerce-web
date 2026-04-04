using EcommerceApp.Models;

namespace EcommerceApp.ViewModels;

public class StoreIndexViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];

    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool InStockOnly { get; init; }
    public string SortBy { get; init; } = "newest";

    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
}
