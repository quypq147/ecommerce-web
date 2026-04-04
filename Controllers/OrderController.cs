using EcommerceApp.Models;
using EcommerceApp.Data;
using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductCatalogService _productCatalogService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public OrderController(
        ICartService cartService,
        IProductCatalogService productCatalogService,
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _cartService = cartService;
        _productCatalogService = productCatalogService;
        _context = context;
        _userManager = userManager;
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
    public async Task<IActionResult> PlaceOrder(string customerName, string customerEmail, string shippingAddress, string phoneNumber)
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
            ModelState.AddModelError("", "Tat ca cac cho trong can duoc dien");
            return RedirectToAction(nameof(Checkout));
        }

        // Calculate total
        var items = cartItems
            .Select(item => new { Product = _productCatalogService.GetProductById(item.Key), item.Value })
            .Where(x => x.Product is not null)
            .ToList();

        var totalAmount = items.Sum(x => x.Product!.Price * x.Value);

        // Create order object (in real app, save to database)
        var currentUserId = _userManager.GetUserId(User);

        var order = new Order
        {
            UserId = currentUserId,
            CustomerEmail = customerEmail,
            RecipientName = customerName,
            PhoneNumber = phoneNumber,
            AddressLine1 = shippingAddress,
            City = "Unknown",
            Country = "Vietnam",
            TotalAmount = totalAmount,
            CreatedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = items.Select(x => new OrderItem
            {
                ProductId = x.Product!.Id,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.Price,
                Quantity = x.Value
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        TempData["OrderId"] = order.Id;

        _cartService.Clear();

        return RedirectToAction(nameof(Confirmation));
    }

    public async Task<IActionResult> Confirmation()
    {
        if (TempData["OrderId"] is not int orderId)
        {
            return RedirectToAction("Index", "Store");
        }

        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
        {
            return RedirectToAction("Index", "Store");
        }

        ViewBag.OrderId = $"ORD-{order.Id:D6}";

        return View(order);
    }
}
