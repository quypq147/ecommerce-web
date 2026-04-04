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
        var items = BuildCartItems();
		if (!items.Any()) return RedirectToAction("Index", "Cart");

		var model = new CheckoutViewModel { CartItems = items };
		return View(model);
	}

	// POST: /Checkout/Process
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Process(CheckoutViewModel model)
	{
     var items = BuildCartItems();
		if (!items.Any()) return RedirectToAction("Index", "Cart");

		model.CartItems = items;
		if (!ModelState.IsValid)
		{
			return View("Index", model);
		}

		var successModel = new CheckoutSuccessViewModel
		{
			OrderCode = $"ORD-{DateTime.Now:yyMMddHHmmss}",
          CustomerName = string.IsNullOrWhiteSpace(model.RecipientName) ? model.FullName : model.RecipientName,
			Items = items
		};

		_cartService.Clear();
     return View("Success", successModel);
	}

	private List<CartItemViewModel> BuildCartItems()
	{
		var cartItems = _cartService.GetItems();
		return cartItems
			.Select(item => new { Product = _productCatalogService.GetProductById(item.Key), Quantity = item.Value })
			.Where(x => x.Product != null)
			.Select(x => new CartItemViewModel { Product = x.Product!, Quantity = x.Quantity })
			.ToList();
	}

}