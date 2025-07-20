using Artisans.Models;
using System.Collections.Generic;

namespace Artisans.Models
{
    public class ArtisanDashboardViewModel
    {
        public ArtisanProfile? ArtisanProfile { get; set; }
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Material> Materials { get; set; } = new List<Material>();
    }
}