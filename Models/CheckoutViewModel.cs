namespace EcommerceApp.ViewModels;

public class CheckoutViewModel
{
	// Thông tin giỏ hàng để hiển thị ở cột bên phải
	public List<CartItemViewModel> CartItems { get; set; } = new();
	public decimal TotalAmount => CartItems.Sum(i => i.Product.Price * i.Quantity);

	// Thông tin thanh toán (Người dùng nhập)
	public string FullName { get; set; } = string.Empty;
	public string CardNumber { get; set; } = string.Empty;
	public string ExpiryDate { get; set; } = string.Empty;
	public string CVV { get; set; } = string.Empty;
	public string PaymentMethod { get; set; } = "Card"; // "Card" hoặc "COD"
}