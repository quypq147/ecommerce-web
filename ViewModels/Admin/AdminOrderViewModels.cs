using EcommerceApp.Models;

namespace EcommerceApp.ViewModels.Admin
{
    public class AdminOrdersViewModel
    {
        public string Search { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<AdminOrderListItemViewModel> Orders { get; set; } = [];
    }

    public class AdminOrderListItemViewModel
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public OrderStatus Status { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AdminOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? StateOrProvince { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<AdminOrderItemViewModel> Items { get; set; } = [];
    }

    public class AdminOrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
