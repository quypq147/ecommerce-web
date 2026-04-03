using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.ViewModels.Admin
{
    public class AdminCategoriesViewModel
    {
        public List<AdminCategoryListItemViewModel> Categories { get; set; } = [];
        public AdminCategoryFormViewModel NewCategory { get; set; } = new();
    }

    public class AdminCategoryListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
    }

    public class AdminCategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
