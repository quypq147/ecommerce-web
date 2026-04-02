using EcommerceApp.Models;

namespace EcommerceApp.Services;

public interface IProductCatalogService
{
    IReadOnlyList<Product> GetProducts();
    Product? GetProductById(int id);
}
