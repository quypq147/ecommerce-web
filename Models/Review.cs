using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
