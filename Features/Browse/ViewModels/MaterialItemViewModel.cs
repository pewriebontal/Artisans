
namespace Artisans.Features.Browse.ViewModels
{
    public class MaterialItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int SupplierArtisanProfileId { get; set; }
        public string SupplierBrandName { get; set; } = string.Empty;
    }
}