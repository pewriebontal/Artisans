
using Artisans.Core.Entities; 

namespace Artisans.Features.Browse.ViewModels
{
    public class ProductItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DescriptionSummary { get; set; } 
        public decimal Price { get; set; }
        public string? MainImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int ArtisanProfileId { get; set; }
        public string ArtisanBrandName { get; set; } = string.Empty;
        public bool IsActive { get; set; } 
    }
}