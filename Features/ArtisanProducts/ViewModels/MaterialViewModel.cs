using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Artisans.Features.ArtisanProducts.ViewModels
{
    public class MaterialViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 1000000.00)]
        [DataType(DataType.Currency)]
        [Display(Name = "Price Per Unit")]
        public decimal PricePerUnit { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Unit of Measure (e.g., meter, piece)")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000)]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public SelectList? Categories { get; set; }

        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1024)]
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}