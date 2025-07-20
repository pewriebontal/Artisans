using System.ComponentModel.DataAnnotations;

namespace Artisans.Models
{
    public class InfluencerPost
    {
        public int Id { get; set; } 

        [Required]
        public int InfluencerUserId { get; set; } 
        public virtual User InfluencerUser { get; set; } = null!; 

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Caption { get; set; }

        public DateTime UploadTimestamp { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovalTimestamp { get; set; }
        public int? ApprovedByAdminUserId { get; set; } 

        public virtual ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
    }
}