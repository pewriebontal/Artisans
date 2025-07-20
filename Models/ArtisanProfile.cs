using System.ComponentModel.DataAnnotations;

namespace Artisans.Models
{
    public class ArtisanProfile
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } 
        public virtual User User { get; set; } = null!; 

        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsApproved { get; set; } = false; 
        public DateTime? ApprovedDate { get; set; }

        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}