
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Artisans.Features.Browse.ViewModels
{
    public class BrowseMaterialsViewModel
    {
        public IEnumerable<MaterialItemViewModel> Materials { get; set; } = new List<MaterialItemViewModel>();
        public SelectList? Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        
    }
}