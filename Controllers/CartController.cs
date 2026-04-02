using EcommerceApp.ViewModels;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;
using EcommerceApp.Models;
using EcommerceApp.ViewModels;

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

	
	// 1. GET: Hiển thị trang nhập thông tin thanh toán
	[HttpGet]
	public IActionResult Checkout()
	{
		var cartItems = _cartService.GetItems();
		var items = cartItems
			.Select(item => new { Product = _productCatalogService.GetProductById(item.Key), Quantity = item.Value })
			.Where(x => x.Product is not null)
			.Select(x => new CartItemViewModel
			{
				Product = x.Product!,
				Quantity = x.Quantity
			})
			.ToList();

		if (!items.Any()) return RedirectToAction(nameof(Index));

		var model = new CheckoutViewModel
		{
			CartItems = items,
			TotalAmount = items.Sum(x => x.Product.Price * x.Quantity)
		};

		return View(model);
	}

	// 2. POST: Xử lý khi nhấn nút "Thanh toán" trên Form
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult ProcessCheckout(CheckoutViewModel model)
	{
		// Thực hiện logic xóa giỏ hàng sau khi thanh toán thành công
		_cartService.Clear();
		TempData["Message"] = "Order placed successfully.";

		return RedirectToAction(nameof(Success));
	}

	// 3. GET: Trang báo thành công
	public IActionResult Success()
	{
		return View();
	}
}