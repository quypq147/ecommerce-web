using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.ViewModels;

public class CheckoutViewModel : IValidatableObject
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

	// Thông tin giao hàng (map theo Order model)
	public string RecipientName { get; set; } = string.Empty;
	public string CustomerEmail { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public string AddressLine1 { get; set; } = string.Empty;
	public string? AddressLine2 { get; set; }
	public string City { get; set; } = string.Empty;
	public string? StateOrProvince { get; set; }
	public string? PostalCode { get; set; }
	public string Country { get; set; } = "Vietnam";

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (string.IsNullOrWhiteSpace(RecipientName))
			yield return new ValidationResult("Vui lòng nhập người nhận.", [nameof(RecipientName)]);

		if (string.IsNullOrWhiteSpace(CustomerEmail) || !new EmailAddressAttribute().IsValid(CustomerEmail))
			yield return new ValidationResult("Email không hợp lệ.", [nameof(CustomerEmail)]);

		if (string.IsNullOrWhiteSpace(PhoneNumber) || !System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^(\+?84|0)\d{9,10}$"))
			yield return new ValidationResult("Số điện thoại không hợp lệ.", [nameof(PhoneNumber)]);

		if (string.IsNullOrWhiteSpace(AddressLine1))
			yield return new ValidationResult("Vui lòng nhập địa chỉ nhận hàng.", [nameof(AddressLine1)]);

		if (string.IsNullOrWhiteSpace(City))
			yield return new ValidationResult("Vui lòng nhập thành phố.", [nameof(City)]);

		if (string.IsNullOrWhiteSpace(Country))
			yield return new ValidationResult("Vui lòng nhập quốc gia.", [nameof(Country)]);

		if (PaymentMethod == "Card")
		{
			if (string.IsNullOrWhiteSpace(FullName))
				yield return new ValidationResult("Vui lòng nhập tên chủ thẻ.", [nameof(FullName)]);

			if (string.IsNullOrWhiteSpace(CardNumber) || !System.Text.RegularExpressions.Regex.IsMatch(CardNumber.Replace(" ", ""), @"^\d{16}$"))
				yield return new ValidationResult("Số thẻ phải gồm 16 chữ số.", [nameof(CardNumber)]);

			if (string.IsNullOrWhiteSpace(ExpiryDate) || !System.Text.RegularExpressions.Regex.IsMatch(ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
				yield return new ValidationResult("Ngày hết hạn phải có định dạng MM/YY.", [nameof(ExpiryDate)]);

			if (string.IsNullOrWhiteSpace(CVV) || !System.Text.RegularExpressions.Regex.IsMatch(CVV, @"^\d{3,4}$"))
				yield return new ValidationResult("CVV phải gồm 3 hoặc 4 chữ số.", [nameof(CVV)]);
		}
	}
}