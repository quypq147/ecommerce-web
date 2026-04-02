using EcommerceApp.Models;

namespace EcommerceApp.Services;

public class InMemoryProductCatalogService : IProductCatalogService
{
    private static readonly List<Category> Categories =
    [
        new Category { Id = 1, Name = "Laptops", Description = "Performance laptops" },
        new Category { Id = 2, Name = "Gaming", Description = "Gaming accessories" },
        new Category { Id = 3, Name = "Components", Description = "PC components" }
    ];

    private static readonly List<Product> Products =
    [
        new Product
        {
            Id = 1,
            Name = "Apex Pro 14",
            Description = "14-inch productivity laptop with Intel Core Ultra and 16GB RAM.",
            Price = 1199.00m,
            ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853",
            StockQuantity = 10,
            CategoryId = 1,
            Category = Categories[0]
        },
        new Product
        {
            Id = 2,
            Name = "Nebula X 16",
            Description = "16-inch gaming laptop with RTX graphics and 240Hz display.",
            Price = 1899.00m,
            ImageUrl = "https://images.unsplash.com/photo-1593642633279-1796119d5482",
            StockQuantity = 7,
            CategoryId = 1,
            Category = Categories[0]
        },
        new Product
        {
            Id = 3,
            Name = "Falcon RGB Mouse",
            Description = "Ergonomic gaming mouse with customizable RGB profiles.",
            Price = 79.00m,
            ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7",
            StockQuantity = 30,
            CategoryId = 2,
            Category = Categories[1]
        },
        new Product
        {
            Id = 4,
            Name = "Titan 32GB DDR5",
            Description = "High-speed memory kit for gaming and creator workloads.",
            Price = 169.00m,
            ImageUrl = "https://images.unsplash.com/photo-1562976540-1502c2145186",
            StockQuantity = 25,
            CategoryId = 3,
            Category = Categories[2]
        }
    ];

    static InMemoryProductCatalogService()
    {
        foreach (var category in Categories)
        {
            category.Products = Products.Where(p => p.CategoryId == category.Id).ToList();
        }
    }

    public IReadOnlyList<Product> GetProducts() => Products;

    public Product? GetProductById(int id) => Products.FirstOrDefault(p => p.Id == id);
}
