using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artisans.Core.Entities
{
    public class Product
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int ArtisanProfileId { get; set; } // Foreign Key to ArtisanProfile
        public virtual ArtisanProfile ArtisanProfile { get; set; } = null!; // Navigation Property

        public int? CategoryId { get; set; } // Foreign Key to Category (nullable if some products don't fit categories)
        public virtual Category? Category { get; set; } // Navigation Property

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;
        public string? MainImageUrl { get; set; }

        // "Journey of Creation" fields
        public string? StoryDetailsText { get; set; }
        public string? ProcessImageUrl1 { get; set; }
        public string? ProcessImageUrl2 { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // For soft delete/hiding

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}