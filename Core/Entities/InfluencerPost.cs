using System.ComponentModel.DataAnnotations;

namespace Artisans.Core.Entities
{
    public class InfluencerPost
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int InfluencerUserId { get; set; } // Foreign Key to User (with Influencer role)
        public virtual User InfluencerUser { get; set; } = null!; // Navigation Property

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Caption { get; set; }

        public DateTime UploadTimestamp { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovalTimestamp { get; set; }
        public int? ApprovedByAdminUserId { get; set; } // Optional: track who approved

        public virtual ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
    }
}