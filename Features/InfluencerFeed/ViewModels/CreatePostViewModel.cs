
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Artisans.Features.InfluencerFeed.ViewModels
{
    public class CreatePostViewModel
    {
        [Required]
        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1024)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Caption { get; set; }

        [Display(Name = "Tag an Artisan Brand")]
        public int? TaggedArtisanProfileId1 { get; set; } 

        [Display(Name = "Tag another Artisan Brand (Optional)")]
        public int? TaggedArtisanProfileId2 { get; set; }

        public SelectList? AvailableArtisanProfiles { get; set; }
    }
}