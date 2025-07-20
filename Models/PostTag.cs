using System.ComponentModel.DataAnnotations;

namespace Artisans.Models
{
    public class PostTag
    {
        public int Id { get; set; } 

        [Required]
        public int InfluencerPostId { get; set; } 
        public virtual InfluencerPost InfluencerPost { get; set; } = null!; 

        [Required]
        public int TaggedArtisanProfileId { get; set; } 
        public virtual ArtisanProfile TaggedArtisanProfile { get; set; } = null!; 

        // If wanted to tag specific products/materials instead of/in addition to brands:
        // public int? TaggedProductId { get; set; }
        // public virtual Product? TaggedProduct { get; set; }
        // public int? TaggedMaterialId { get; set; }
        // public virtual Material? TaggedMaterial { get; set; }
    }
}