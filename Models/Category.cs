using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(80)]
        public string? Icon { get; set; }

        // Navigation property
        public List<Product> Products { get; set; }
    }
}