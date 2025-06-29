using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Artisans.Core.Entities;

namespace Artisans.Features.ArtisanProducts.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; } 

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(4000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000000.00)]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public SelectList? Categories { get; set; } 

        [Display(Name = "Main Image URL")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1024)]
        public string? MainImageUrl { get; set; }

        [Display(Name = "Story / Creation Details")]
        [DataType(DataType.MultilineText)]
        public string? StoryDetailsText { get; set; }

        [Display(Name = "Process Image URL 1")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1024)]
        public string? ProcessImageUrl1 { get; set; }

        [Display(Name = "Process Image URL 2")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1024)]
        public string? ProcessImageUrl2 { get; set; }

        public bool IsActive { get; set; } = true;
    }
}