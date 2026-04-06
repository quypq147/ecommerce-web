using EcommerceApp.Models;

namespace EcommerceApp.ViewModels;

public class WishlistViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = [];
}
