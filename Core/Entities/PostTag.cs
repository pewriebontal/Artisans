using System.ComponentModel.DataAnnotations;

namespace Artisans.Core.Entities
{
    public class PostTag
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int InfluencerPostId { get; set; } // Foreign Key to InfluencerPost
        public virtual InfluencerPost InfluencerPost { get; set; } = null!; // Navigation Property

        [Required]
        public int TaggedArtisanProfileId { get; set; } // Foreign Key to ArtisanProfile (the brand being tagged)
        public virtual ArtisanProfile TaggedArtisanProfile { get; set; } = null!; // Navigation Property

        // If wanted to tag specific products/materials instead of/in addition to brands:
        // public int? TaggedProductId { get; set; }
        // public virtual Product? TaggedProduct { get; set; }
        // public int? TaggedMaterialId { get; set; }
        // public virtual Material? TaggedMaterial { get; set; }
    }
}