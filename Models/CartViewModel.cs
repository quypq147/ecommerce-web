namespace EcommerceApp.Models;

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.LineTotal);
}
