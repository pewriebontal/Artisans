using Artisans.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artisans.Core.Entities
{
    public class Order
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int BuyerUserId { get; set; } // Foreign Key to User
        public virtual User BuyerUser { get; set; } = null!; // Navigation Property

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Simplified shipping info for now
        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;
        [StringLength(100)]
        public string? ShippingCity { get; set; }
        [StringLength(10)]
        public string? ShippingPostalCode { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}