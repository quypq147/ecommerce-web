namespace EcommerceApp.Models;

using EcommerceApp.ViewModels;

public class CheckoutViewModel
{
    public List<CartItemViewModel> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public bool SaveInfo { get; set; }
}