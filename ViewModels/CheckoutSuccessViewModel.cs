namespace EcommerceApp.ViewModels;

public class CheckoutSuccessViewModel
{
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];
    public decimal Subtotal => Items.Sum(x => x.LineTotal);
}
