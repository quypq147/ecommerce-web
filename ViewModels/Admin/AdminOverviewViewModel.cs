using EcommerceApp.Models;

namespace EcommerceApp.ViewModels.Admin
{
    public class AdminOverviewViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public List<AdminOrderListItemViewModel> RecentOrders { get; set; } = [];
        public List<Product> LowStockProducts { get; set; } = [];
    }
}
