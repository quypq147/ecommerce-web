using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcommerceApp.ViewModels.Admin
{
    public class AdminProductFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(typeof(decimal), "0", "99999999")]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(1000)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Ảnh sản phẩm")]
        public IFormFile? ImageFile { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Stock")]
        public int StockQuantity { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<CategoryOptionViewModel> Categories { get; set; } = [];
    }

    public class CategoryOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
