using EcommerceApp.ViewModels;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductCatalogService _productCatalogService;

    public CartController(ICartService cartService, IProductCatalogService productCatalogService)
    {
        _cartService = cartService;
        _productCatalogService = productCatalogService;
    }

    public IActionResult Index()
    {
        var cartItems = _cartService.GetItems();

        var items = cartItems
            .Select(item => new { Product = _productCatalogService.GetProductById(item.Key), item.Value })
            .Where(x => x.Product is not null)
            .Select(x => new CartItemViewModel
            {
                Product = x.Product!,
                Quantity = x.Value
            })
            .ToList();

        var model = new CartViewModel
        {
            Items = items
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        _cartService.Update(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cartService.Remove(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout()
    {
        return RedirectToAction("Checkout", "Order");
    }
}
