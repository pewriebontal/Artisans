using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artisans.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int OrderId { get; set; } // Foreign Key to Order
        public virtual Order Order { get; set; } = null!; // Navigation Property

        public int? ProductId { get; set; } // Foreign Key to Product (nullable)
        public virtual Product? Product { get; set; } // Navigation Property

        public int? MaterialId { get; set; } // Foreign Key to Material (nullable)
        public virtual Material? Material { get; set; } // Navigation Property

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceAtPurchase { get; set; } // Price at the time of purchase
    }
}