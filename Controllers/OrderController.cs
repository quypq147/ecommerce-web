using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class OrderController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductCatalogService _productCatalogService;

    public OrderController(ICartService cartService, IProductCatalogService productCatalogService)
    {
        _cartService = cartService;
        _productCatalogService = productCatalogService;
    }

    public IActionResult Checkout()
    {
        var cartItems = _cartService.GetItems();

        if (cartItems.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

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
    public IActionResult PlaceOrder(string customerName, string customerEmail, string shippingAddress, string phoneNumber)
    {
        var cartItems = _cartService.GetItems();

        if (cartItems.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerEmail) ||
            string.IsNullOrWhiteSpace(shippingAddress) || string.IsNullOrWhiteSpace(phoneNumber))
        {
            ModelState.AddModelError("", "All fields are required");
            return RedirectToAction(nameof(Checkout));
        }

        // Calculate total
        var items = cartItems
            .Select(item => new { Product = _productCatalogService.GetProductById(item.Key), item.Value })
            .Where(x => x.Product is not null)
            .ToList();

        var totalAmount = items.Sum(x => x.Product!.Price * x.Value);

        // Create order object (in real app, save to database)
        var order = new Order
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            ShippingAddress = shippingAddress,
            PhoneNumber = phoneNumber,
            TotalAmount = totalAmount,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        // Store order in TempData for display (in real app, save to DB and get ID)
        TempData["OrderId"] = "ORD-" + DateTime.UtcNow.Ticks;
        TempData["OrderDetails"] = System.Text.Json.JsonSerializer.Serialize(order);

        _cartService.Clear();

        return RedirectToAction(nameof(Confirmation));
    }

    public IActionResult Confirmation()
    {
        if (TempData["OrderDetails"] is not string orderJson)
        {
            return RedirectToAction("Index", "Store");
        }

        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(orderJson);
        ViewBag.OrderId = TempData["OrderId"];

        return View(order);
    }
}
