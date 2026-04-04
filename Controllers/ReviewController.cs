using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

public class ReviewController : Controller
{
    private readonly IProductCatalogService _productCatalogService;

    public ReviewController(IProductCatalogService productCatalogService)
    {
        _productCatalogService = productCatalogService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Review review)
    {
        if (string.IsNullOrWhiteSpace(review.ReviewerName) || string.IsNullOrWhiteSpace(review.Comment))
        {
            ModelState.AddModelError("", "Reviewer name and comment are required");
            return RedirectToAction("Reviews", "Store", new { id = review.ProductId });
        }

        // In real app, save to database
        TempData["ReviewMessage"] = "Thank you! Your review has been submitted successfully!";

        return RedirectToAction("Reviews", "Store", new { id = review.ProductId });
    }
}
