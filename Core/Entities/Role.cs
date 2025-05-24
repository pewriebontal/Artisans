using Microsoft.AspNetCore.Identity;

namespace Artisans.Core.Entities
{
    public class Role : IdentityRole<int> // Use <int> for key type
    {
        // public string Description { get; set; }
    }
}