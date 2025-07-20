using Microsoft.AspNetCore.Identity;
using Artisans.Models;

namespace Artisans.Models
{
    // Inherit from IdentityUser<int>
    public class User : IdentityUser<int>
    {
        // IdentityUser already provides:
        // Id (will be int due to <int>)
        // UserName
        // Email
        // PasswordHash
        // PhoneNumber, etc.

        public UserRoleType CustomRole { get; set; } 
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ArtisanProfile? ArtisanProfile { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<InfluencerPost> InfluencerPosts { get; set; } = new List<InfluencerPost>();
    }
}