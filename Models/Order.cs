using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }

    public class Order
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string? UserId { get; set; }

        [Required]
        [StringLength(256)]
        public string CustomerEmail { get; set; } = string.Empty;


        [Required]
        [StringLength(150)]
        public string RecipientName { get; set; } = string.Empty;

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? StateOrProvince { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(30)]
        public string PaymentMethod { get; set; } = "COD";

        [StringLength(50)]
        public string? PaymentProvider { get; set; }

        [StringLength(100)]
        public string? PaymentTransactionId { get; set; }

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public DateTime? PaidAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public List<OrderItem> Items { get; set; } = [];
    }
}
