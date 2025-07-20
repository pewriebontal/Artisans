using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artisans.Models
{
    public class Material
    {
        public int Id { get; set; } 

        [Required]
        public int SupplierArtisanProfileId { get; set; } 
        public virtual ArtisanProfile SupplierArtisanProfile { get; set; } = null!; 

        public int? CategoryId { get; set; } 
        public virtual Category? Category { get; set; } 

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }

        [Required]
        [StringLength(50)]
        public string UnitOfMeasure { get; set; } = string.Empty; 

        public int StockQuantity { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}