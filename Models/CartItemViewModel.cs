namespace EcommerceApp.Models;

public class CartItemViewModel
{
    public required Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Product.Price * Quantity;
}
