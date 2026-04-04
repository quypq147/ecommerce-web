using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private const string DisabledPrefix = "[DISABLED] ";
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description ?? string.Empty).Contains(search) ||
                    p.Category.Name.Contains(search));
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Search = search ?? string.Empty,
                Products = products,
                TotalCatalog = products.Count,
                ActiveStock = products.Where(p => !IsDisabled(p)).Sum(p => p.StockQuantity),
                LowInventoryCount = products.Count(p => !IsDisabled(p) && p.StockQuantity <= 15)
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var viewModel = new AdminProductFormViewModel
            {
                Categories = await GetCategoryOptionsAsync()
            };

            return View("ProductForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(AdminProductFormViewModel model)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoryOptionsAsync();
                return View("ProductForm", model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = BuildDescription(model.Description, disable: false),
                Price = model.Price,
                ImageUrl = model.ImageUrl ?? string.Empty,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            var viewModel = new AdminProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = CleanDescription(product.Description),
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                Categories = await GetCategoryOptionsAsync()
            };

            return View("ProductForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(AdminProductFormViewModel model)
        {
            if (!model.Id.HasValue)
            {
                return BadRequest();
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.Id.Value);
            if (product is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoryOptionsAsync();
                return View("ProductForm", model);
            }

            product.Name = model.Name;
            product.Description = BuildDescription(model.Description, IsDisabled(product));
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl ?? string.Empty;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            if (!IsDisabled(product))
            {
                product.Description = BuildDescription(product.Description, disable: true);
                await _context.SaveChangesAsync();
                TempData["AdminMessage"] = "Product disabled.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            if (IsDisabled(product))
            {
                product.Description = BuildDescription(product.Description, disable: false);
                await _context.SaveChangesAsync();
                TempData["AdminMessage"] = "Product enabled.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is not null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["AdminMessage"] = "Product deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Select(c => new AdminCategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new AdminCategoriesViewModel
            {
                Categories = categories,
                NewCategory = new AdminCategoryFormViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(AdminCategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories
                    .Select(c => new AdminCategoryListItemViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ProductCount = c.Products.Count
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View("Categories", new AdminCategoriesViewModel
                {
                    Categories = categories,
                    NewCategory = model
                });
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                Products = []
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            var viewModel = new AdminCategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(AdminCategoryFormViewModel model)
        {
            if (!model.Id.HasValue)
            {
                return BadRequest();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id.Value);
            if (category is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            category.Name = model.Name;
            category.Description = model.Description ?? string.Empty;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["AdminMessage"] = "Cannot delete category with existing products.";
                return RedirectToAction(nameof(Categories));
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is not null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["AdminMessage"] = "Category deleted.";
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string? search = null, string? status = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.CustomerEmail.Contains(search) ||
                    o.RecipientName.Contains(search) ||
                    o.Id.ToString().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync();

            var viewModel = new AdminOrdersViewModel
            {
                Search = search ?? string.Empty,
                StatusFilter = status ?? string.Empty,
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                Orders = orders.Select(o => new AdminOrderListItemViewModel
                {
                    Id = o.Id,
                    CustomerEmail = o.CustomerEmail,
                    RecipientName = o.RecipientName,
                    CreatedAtUtc = o.CreatedAtUtc,
                    Status = o.Status,
                    ItemCount = o.Items.Sum(i => i.Quantity),
                    TotalAmount = o.TotalAmount
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return View(MapOrderDetails(order));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null)
            {
                return NotFound();
            }

            order.Status = status;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Order status updated successfully.";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderAddress(
            int orderId,
            string recipientName,
            string addressLine1,
            string? addressLine2,
            string city,
            string? stateOrProvince,
            string? postalCode,
            string country,
            string? phoneNumber)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(recipientName) ||
                string.IsNullOrWhiteSpace(addressLine1) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(country))
            {
                TempData["AdminMessage"] = "Recipient, address line 1, city, and country are required.";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }

            order.RecipientName = recipientName.Trim();
            order.AddressLine1 = addressLine1.Trim();
            order.AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
            order.City = city.Trim();
            order.StateOrProvince = string.IsNullOrWhiteSpace(stateOrProvince) ? null : stateOrProvince.Trim();
            order.PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
            order.Country = country.Trim();
            order.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
            order.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Shipping address updated successfully.";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        private Task<List<CategoryOptionViewModel>> GetCategoryOptionsAsync()
        {
            return _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        private static bool IsDisabled(Product product)
        {
            return (product.Description ?? string.Empty).StartsWith(DisabledPrefix, StringComparison.Ordinal);
        }

        private static string CleanDescription(string? description)
        {
            var value = description ?? string.Empty;
            return value.StartsWith(DisabledPrefix, StringComparison.Ordinal)
                ? value[DisabledPrefix.Length..]
                : value;
        }

        private static string BuildDescription(string? description, bool disable)
        {
            var clean = CleanDescription(description);
            return disable ? $"{DisabledPrefix}{clean}" : clean;
        }

        private static AdminOrderDetailsViewModel MapOrderDetails(Order order)
        {
            return new AdminOrderDetailsViewModel
            {
                Id = order.Id,
                CustomerEmail = order.CustomerEmail,
                RecipientName = order.RecipientName,
                PhoneNumber = order.PhoneNumber,
                AddressLine1 = order.AddressLine1,
                AddressLine2 = order.AddressLine2,
                City = order.City,
                StateOrProvince = order.StateOrProvince,
                PostalCode = order.PostalCode,
                Country = order.Country,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new AdminOrderItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }
    }
}
