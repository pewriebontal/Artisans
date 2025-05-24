using Microsoft.AspNetCore.Identity;
using Artisans.Core.Enums;

namespace Artisans.Core.Entities
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

        // Custom properties we want to keep:
        public UserRoleType CustomRole { get; set; } // We'll manage this via Identity Roles system primarily,
                                                     // but can keep it for easy access if needed, or remove if redundant
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ArtisanProfile? ArtisanProfile { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<InfluencerPost> InfluencerPosts { get; set; } = new List<InfluencerPost>();
    }
}