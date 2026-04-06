using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IProductCatalogService _productCatalogService;
    private readonly ICartService _cartService;

    public WishlistController(IWishlistService wishlistService, IProductCatalogService productCatalogService, ICartService cartService)
    {
        _wishlistService = wishlistService;
        _productCatalogService = productCatalogService;
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        var products = _wishlistService.GetItems()
            .Select(id => _productCatalogService.GetProductById(id))
            .Where(product => product is not null)
            .Select(product => product!)
            .ToList();

        return View(new WishlistViewModel
        {
            Products = products
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int productId, string? returnUrl)
    {
        if (_productCatalogService.GetProductById(productId) is null)
        {
            return NotFound();
        }

        _wishlistService.Add(productId);
        TempData["Message"] = "Sản phẩm đã được thêm vào danh sách yêu thích.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int productId, string? returnUrl)
    {
        if (_productCatalogService.GetProductById(productId) is null)
        {
            return NotFound();
        }

        _cartService.Add(productId);
        _wishlistService.Remove(productId);
        TempData["Message"] = "Sản phẩm đã được chuyển sang giỏ hàng.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _wishlistService.Remove(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _wishlistService.Clear();
        TempData["Message"] = "Danh sách yêu thích đã được xóa.";
        return RedirectToAction(nameof(Index));
    }
}
