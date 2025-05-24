using System.ComponentModel.DataAnnotations;

namespace Artisans.Core.Entities
{
    public class ArtisanProfile
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int UserId { get; set; } // Foreign Key to User
        public virtual User User { get; set; } = null!; // Navigation Property

        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsApproved { get; set; } = false; // Admin approval for artisans
        public DateTime? ApprovedDate { get; set; }

        // Navigation property for products/materials by this artisan
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}