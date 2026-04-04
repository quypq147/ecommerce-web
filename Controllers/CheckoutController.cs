using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class CheckoutController : Controller
{
	private readonly ICartService _cartService;
	private readonly IProductCatalogService _productCatalogService;

	public CheckoutController(ICartService cartService, IProductCatalogService productCatalogService)
	{
		_cartService = cartService;
		_productCatalogService = productCatalogService;
	}

	// GET: /Checkout
	public IActionResult Index()
	{
		var cartItems = _cartService.GetItems();
		if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

		var items = cartItems
			.Select(item => new { Product = _productCatalogService.GetProductById(item.Key), Quantity = item.Value })
			.Where(x => x.Product != null)
			.Select(x => new CartItemViewModel { Product = x.Product!, Quantity = x.Quantity })
			.ToList();

		var model = new CheckoutViewModel { CartItems = items };
		return View(model);
	}

	// POST: /Checkout/Process
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Process(CheckoutViewModel model)
	{
		// Tại đây bạn xử lý lưu Database hoặc gọi API thanh toán
		// Giả sử thanh toán thành công:
		_cartService.Clear();
		return View("Success"); // Tạo thêm View Success nếu muốn hiện thông báo cảm ơn
	}

}