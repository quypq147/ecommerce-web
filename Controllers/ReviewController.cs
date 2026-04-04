using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers;

public class ReviewController : Controller
{
    private readonly IProductCatalogService _productCatalogService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ReviewController(
        IProductCatalogService productCatalogService,
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _productCatalogService = productCatalogService;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review review, CancellationToken cancellationToken)
    {
        if (_productCatalogService.GetProductById(review.ProductId) is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(review.Comment) || review.Rating is < 1 or > 5)
        {
            TempData["ReviewMessage"] = "Vui lòng nhập bình luận và điểm đánh giá hợp lệ (1-5).";
            return RedirectToAction("Reviews", "Store", new { id = review.ProductId });
        }

        var userId = _userManager.GetUserId(User);
        var userEmail = User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(userEmail))
        {
            return Challenge();
        }

        var hasPurchased = await _context.OrderItems
            .AsNoTracking()
            .AnyAsync(oi =>
                oi.ProductId == review.ProductId
                && oi.Order.Status == OrderStatus.Delivered
                && (
                    (!string.IsNullOrWhiteSpace(userId) && oi.Order.UserId == userId)
                    || (!string.IsNullOrWhiteSpace(userEmail) && oi.Order.CustomerEmail == userEmail)
                ), cancellationToken);

        if (!hasPurchased)
        {
            TempData["ReviewMessage"] = "Bạn chỉ có thể đánh giá sau khi đã mua và nhận sản phẩm.";
            return RedirectToAction("Reviews", "Store", new { id = review.ProductId });
        }

        var reviewerName = User.Identity?.Name;

        var newReview = new Review
        {
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment.Trim(),
            ReviewerName = string.IsNullOrWhiteSpace(reviewerName) ? review.ReviewerName.Trim() : reviewerName,
            CreatedDate = DateTime.UtcNow
        };

        _context.Reviews.Add(newReview);
        await _context.SaveChangesAsync(cancellationToken);

        var ratingSummary = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == review.ProductId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Average = g.Average(x => (decimal)x.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == review.ProductId, cancellationToken);
        if (product is not null)
        {
            product.AverageRating = Math.Round(ratingSummary?.Average ?? 0m, 2, MidpointRounding.AwayFromZero);
            product.ReviewCount = ratingSummary?.Count ?? 0;
            await _context.SaveChangesAsync(cancellationToken);
        }

        TempData["ReviewMessage"] = "Cảm ơn bạn! Đánh giá đã được ghi nhận.";

        return RedirectToAction("Reviews", "Store", new { id = review.ProductId });
    }
}
