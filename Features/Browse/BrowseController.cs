
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.Infrastructure.Data;
using Artisans.Core.Entities; 
using System.Linq;
using System.Threading.Tasks;
using Artisans.Features.Browse.ViewModels; 
using Artisans.Features.ArtisanProducts.ViewModels; 
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Artisans.Features.Browse
{
    public class BrowseController : Controller
    {
        private readonly ArtisansDBContext _context;

        public BrowseController(ArtisansDBContext context)
        {
            _context = context;
        }

        
        
        public async Task<IActionResult> Products(int? categoryId, string? searchQuery)
        {
            IQueryable<Product> productsQuery = _context.Products
                                                   .Include(p => p.Category)
                                                   .Include(p => p.ArtisanProfile) 
                                                   .Where(p => p.IsActive && p.ArtisanProfile.IsApproved); 

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                ViewData["SelectedCategory"] = await _context.Categories.FindAsync(categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchQuery) ||
                                                         p.Description.Contains(searchQuery) ||
                                                         (p.Category != null && p.Category.Name.Contains(searchQuery)) ||
                                                         p.ArtisanProfile.BrandName.Contains(searchQuery));
                ViewData["SearchQuery"] = searchQuery;
            }
            
            ViewBag.AllCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(); 

            var products = await productsQuery.OrderByDescending(p => p.DateAdded).ToListAsync();
            return View(products); 
        }

        
        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                    .Include(p => p.Category)
                                    .Include(p => p.ArtisanProfile) 
                                    .Where(p => p.IsActive && p.ArtisanProfile.IsApproved) 
                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); 
        }


        
        public async Task<IActionResult> Materials(int? categoryId, string? searchQuery)
        {
            IQueryable<Material> materialsQuery = _context.Materials
                                                       .Include(m => m.Category)
                                                       .Include(m => m.SupplierArtisanProfile) 
                                                       .Where(m => m.IsActive && m.SupplierArtisanProfile.IsApproved);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                materialsQuery = materialsQuery.Where(m => m.CategoryId == categoryId.Value);
                 ViewData["SelectedCategory"] = await _context.Categories.FindAsync(categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                materialsQuery = materialsQuery.Where(m => m.Name.Contains(searchQuery) ||
                                                          (m.Description != null && m.Description.Contains(searchQuery)) ||
                                                          (m.Category != null && m.Category.Name.Contains(searchQuery)) ||
                                                          m.SupplierArtisanProfile.BrandName.Contains(searchQuery));
                ViewData["SearchQuery"] = searchQuery;
            }
            
            ViewBag.AllCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            var materials = await materialsQuery.OrderByDescending(m => m.DateAdded).ToListAsync();
            return View(materials); 
        }

        
        public async Task<IActionResult> MaterialDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                                    .Include(m => m.Category)
                                    .Include(m => m.SupplierArtisanProfile)
                                    .Where(m => m.IsActive && m.SupplierArtisanProfile.IsApproved)
                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material); 
        }

        
        public async Task<IActionResult> ArtisanStore(int? id)
        {
            if (id == null) return NotFound();

            var artisanProfile = await _context.ArtisanProfiles
                                        .Include(ap => ap.Products.Where(p => p.IsActive))
                                            .ThenInclude(p => p.Category)
                                        .Include(ap => ap.Materials.Where(m => m.IsActive))
                                            .ThenInclude(m => m.Category)
                                        .FirstOrDefaultAsync(ap => ap.Id == id && ap.IsApproved);
            
            if (artisanProfile == null) return NotFound();

            
            var viewModel = new ArtisanDashboardViewModel 
            {
                ArtisanProfile = artisanProfile,
                Products = artisanProfile.Products.OrderByDescending(p => p.DateAdded).ToList(),
                Materials = artisanProfile.Materials.OrderByDescending(m => m.DateAdded).ToList()
            };
            return View(viewModel);
        }
    }
}