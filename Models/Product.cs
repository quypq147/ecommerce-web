using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }

        // Foreign Key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}