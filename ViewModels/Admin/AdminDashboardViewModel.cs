using EcommerceApp.Models;

namespace EcommerceApp.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public string Search { get; set; } = string.Empty;
        public int TotalCatalog { get; set; }
        public int ActiveStock { get; set; }
        public int LowInventoryCount { get; set; }
        public List<Product> Products { get; set; } = [];

        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public List<AdminOrderListItemViewModel> RecentOrders { get; set; } = [];
        public List<Product> LowStockProducts { get; set; } = [];

        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = [];
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = [];
    }

    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
