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
    }
}
