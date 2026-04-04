using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Services;

public class DbProductCatalogService : IProductCatalogService
{
    private readonly ApplicationDbContext _context;

    public DbProductCatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<Product> GetProducts()
    {
        return _context.Products
            .Include(p => p.Category)
            .ToList();
    }

    public Product? GetProductById(int id)
    {
        return _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id);
    }
}
