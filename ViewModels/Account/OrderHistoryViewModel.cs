using EcommerceApp.Models;

namespace EcommerceApp.ViewModels.Account;

public class OrderHistoryViewModel
{
    public List<OrderHistoryItemViewModel> Orders { get; set; } = [];
}

public class OrderHistoryItemViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}
