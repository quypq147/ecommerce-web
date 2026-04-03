using EcommerceApp.Services;
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

    public IActionResult Index()
    {
        var products = _productCatalogService.GetProducts();
        return View(products);
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
    public IActionResult AddToCart(int id)
    {
        if (_productCatalogService.GetProductById(id) is null)
        {
            return NotFound();
        }

        _cartService.Add(id);
        TempData["Message"] = "Product added to cart.";

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

