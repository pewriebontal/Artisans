using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artisans.Models
{
    public class OrderItem
    {
        public int Id { get; set; } 

        [Required]
        public int OrderId { get; set; } 
        public virtual Order Order { get; set; } = null!; 

        public int? ProductId { get; set; } 
        public virtual Product? Product { get; set; } 

        public int? MaterialId { get; set; } 
        public virtual Material? Material { get; set; } 

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceAtPurchase { get; set; } 
    }
}