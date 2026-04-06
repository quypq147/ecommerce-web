using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class StoreController : Controller
{
    private readonly IProductCatalogService _productCatalogService;
    private readonly ICartService _cartService;

    public StoreController(IProductCatalogService productCatalogService, ICartService cartService)
    {
        _productCatalogService = productCatalogService;
        _cartService = cartService;
    }

    public IActionResult Index(
        string? searchTerm,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool inStockOnly = false,
        string sortBy = "newest",
        int page = 1)
    {
        var products = _productCatalogService.GetProducts();

        var categories = products
            .Where(p => p.Category is not null)
            .Select(p => p.Category!)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .OrderBy(c => c.Name)
            .ToList();

        var query = products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (inStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
            _ => query.OrderByDescending(p => p.Id)
        };

        const int pageSize = 6;
        var filteredProducts = query.ToList();
        var totalItems = filteredProducts.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var pagedProducts = filteredProducts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new StoreIndexViewModel
        {
            Products = pagedProducts,
            Categories = categories,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStockOnly = inStockOnly,
            SortBy = sortBy,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    public IActionResult Details(int id)
    {
        var product = _productCatalogService.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int id, string? returnUrl)
    {
        if (_productCatalogService.GetProductById(id) is null)
        {
            return NotFound();
        }

        _cartService.Add(id);
        TempData["Message"] = "Sản phẩm đã được thêm vào giỏ.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Reviews(int id)
    {
        var product = _productCatalogService.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }
}

