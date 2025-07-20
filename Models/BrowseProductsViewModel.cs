
using Microsoft.AspNetCore.Mvc.Rendering; 
using System.Collections.Generic;

namespace Artisans.Models
{
    public class BrowseProductsViewModel
    {
        public IEnumerable<ProductItemViewModel> Products { get; set; } = new List<ProductItemViewModel>();
        public SelectList? Categories { get; set; } 
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        
    }
}