using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers;

public class CheckoutController : Controller
{
	private readonly ICartService _cartService;
	private readonly IProductCatalogService _productCatalogService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

	public CheckoutController(
        ICartService cartService,
        IProductCatalogService productCatalogService,
        IPaymentGatewayService paymentGatewayService,
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
	{
		_cartService = cartService;
		_productCatalogService = productCatalogService;
        _paymentGatewayService = paymentGatewayService;
        _context = context;
        _userManager = userManager;
	}

	// GET: /Checkout
    [Authorize]
	public IActionResult Index()
	{
        var items = BuildCartItems();
		if (!items.Any()) return RedirectToAction("Index", "Cart");

		var model = new CheckoutViewModel { CartItems = items };
		return View(model);
	}

	// POST: /Checkout/Process
	[HttpPost]
    [Authorize]
	[ValidateAntiForgeryToken]
	public Task<IActionResult> Process(CheckoutViewModel model, CancellationToken cancellationToken)
	{
        return ProcessPayment(model, cancellationToken);
	}

    // POST: /Checkout/ProcessPayment
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        var items = BuildCartItems();
        if (!items.Any()) return RedirectToAction("Index", "Cart");

        model.CartItems = items;
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var currentUserId = _userManager.GetUserId(User);
        var paymentMethod = string.IsNullOrWhiteSpace(model.PaymentMethod) ? "COD" : model.PaymentMethod.Trim();
        var isOnlinePayment = paymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase)
            || paymentMethod.Equals("STRIPE", StringComparison.OrdinalIgnoreCase);

        var order = new Order
        {
            UserId = currentUserId,
            CustomerEmail = model.CustomerEmail,
            RecipientName = string.IsNullOrWhiteSpace(model.RecipientName) ? model.FullName : model.RecipientName,
            PhoneNumber = model.PhoneNumber,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            StateOrProvince = model.StateOrProvince,
            PostalCode = model.PostalCode,
            Country = model.Country,
            CreatedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentMethod = paymentMethod,
            PaymentProvider = isOnlinePayment ? paymentMethod.ToUpperInvariant() : "OFFLINE",
            PaymentStatus = isOnlinePayment ? PaymentStatus.Pending : PaymentStatus.Unpaid,
            TotalAmount = items.Sum(i => i.Product.Price * i.Quantity),
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.Product.Id,
                ProductName = i.Product.Name,
                UnitPrice = i.Product.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var orderCode = $"ORD-{order.Id:D8}";
        order.PaymentReference = orderCode;

        if (!isOnlinePayment)
        {
            await _context.SaveChangesAsync(cancellationToken);

            var successModel = new CheckoutSuccessViewModel
            {
                OrderCode = orderCode,
                CustomerName = order.RecipientName,
                Items = items
            };

            _cartService.Clear();
            return View("Success", successModel);
        }

        var callbackUrl = Url.Action(nameof(PaymentCallback), "Checkout", new { orderId = order.Id }, Request.Scheme);
        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo URL callback thanh toán.");
            return View("Index", model);
        }

        var initResult = await _paymentGatewayService.InitializeAsync(new PaymentRequest
        {
            Provider = paymentMethod,
            OrderCode = orderCode,
            Amount = order.TotalAmount,
            Description = $"Thanh toan don hang {orderCode}",
            ReturnUrl = callbackUrl
        }, cancellationToken);

        if (!initResult.Success || string.IsNullOrWhiteSpace(initResult.RedirectUrl))
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            ModelState.AddModelError(string.Empty, initResult.ErrorMessage ?? "Không thể khởi tạo thanh toán.");
            return View("Index", model);
        }

        order.PaymentTransactionId = initResult.TransactionId;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Redirect(initResult.RedirectUrl);
    }

    // GET: /Checkout/PaymentCallback
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback(int orderId, string? status, string? provider, string? vnp_TransactionNo, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return RedirectToAction("Index", "Store");
        }

        var isSuccess = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase);

        order.PaymentProvider = string.IsNullOrWhiteSpace(provider) ? order.PaymentProvider : provider;
        order.PaymentTransactionId = string.IsNullOrWhiteSpace(vnp_TransactionNo) ? order.PaymentTransactionId : vnp_TransactionNo;
        order.PaymentStatus = isSuccess ? PaymentStatus.Paid : PaymentStatus.Failed;
        order.Status = isSuccess ? OrderStatus.Processing : OrderStatus.Pending;
        order.PaidAtUtc = isSuccess ? DateTime.UtcNow : null;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (!isSuccess)
        {
            TempData["PaymentError"] = "Thanh toán chưa thành công. Vui lòng thử lại.";
            return RedirectToAction(nameof(Index));
        }

        var successModel = new CheckoutSuccessViewModel
        {
            OrderCode = order.PaymentReference ?? $"ORD-{order.Id:D8}",
            CustomerName = order.RecipientName,
            Items = order.Items.Select(i => new CartItemViewModel
            {
                Product = new Product
                {
                    Id = i.ProductId,
                    Name = i.ProductName,
                    Price = i.UnitPrice
                },
                Quantity = i.Quantity
            }).ToList()
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