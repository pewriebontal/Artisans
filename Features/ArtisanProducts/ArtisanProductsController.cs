using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Artisans.Core.Entities;
using Artisans.Features.ArtisanProducts.ViewModels;
using Artisans.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Artisans.Features.ArtisanProducts
{
    [Authorize(Roles = "Artisan")] // Only allow users in the "Artisan" role
    public class ArtisanProductsController : Controller
    {
        private readonly ArtisansDBContext _context;
        private readonly UserManager<User> _userManager;

        public ArtisanProductsController(ArtisansDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ArtisanProducts (Dashboard for the artisan)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // This should ideally not happen due to [Authorize]
                return Challenge(); 
            }

            var artisanProfile = await _context.ArtisanProfiles
                                        .Include(ap => ap.Products)
                                            .ThenInclude(p => p.Category) 
                                        .Include(ap => ap.Materials)
                                            .ThenInclude(m => m.Category) 
                                        .FirstOrDefaultAsync(ap => ap.UserId == currentUser.Id);

            if (artisanProfile == null)
            {
                TempData["ErrorMessage"] = "Artisan profile not found or not yet set up. Please contact support or complete your profile setup.";
                return RedirectToAction("Index", "Home", new { area = "" }); 
            }
            
            if (!artisanProfile.IsApproved)
            {
                TempData["WarningMessage"] = "Your artisan profile is pending administrator approval. You can manage your items, but they will not be publicly visible until approved.";
            }

            var viewModel = new ArtisanDashboardViewModel
            {
                ArtisanProfile = artisanProfile,
                Products = artisanProfile.Products.OrderByDescending(p => p.DateAdded).ToList(),
                Materials = artisanProfile.Materials.OrderByDescending(m => m.DateAdded).ToList()
            };

            return View(viewModel);
        }

        // Helper method to populate categories for dropdowns
        private async Task PopulateCategoriesDropDownListAsync(object? selectedCategory = null)
        {
            var categoriesQuery = from c in _context.Categories
                                  orderby c.Name
                                  select c;
            ViewBag.Categories = new SelectList(await categoriesQuery.AsNoTracking().ToListAsync(), "Id", "Name", selectedCategory);
        }

        // --- PRODUCT MANAGEMENT ---

        // GET: ArtisanProducts/CreateProduct
        public async Task<IActionResult> CreateProduct()
        {
            await PopulateCategoriesDropDownListAsync();
            var model = new ProductViewModel { IsActive = true };
            return View(model);
        }

        // POST: ArtisanProducts/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);

            if (artisanProfile == null) 
            {
                ModelState.AddModelError("", "Unable to find your artisan profile.");
                await PopulateCategoriesDropDownListAsync(model.CategoryId);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ArtisanProfileId = artisanProfile.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId == 0 ? null : model.CategoryId, // Handle "no category"
                    MainImageUrl = model.MainImageUrl,
                    StoryDetailsText = model.StoryDetailsText,
                    ProcessImageUrl1 = model.ProcessImageUrl1,
                    ProcessImageUrl2 = model.ProcessImageUrl2,
                    IsActive = model.IsActive,
                    DateAdded = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoriesDropDownListAsync(model.CategoryId);
            return View(model);
        }

        // GET: ArtisanProducts/EditProduct/5
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ArtisanProfileId == artisanProfile.Id);
            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                MainImageUrl = product.MainImageUrl,
                StoryDetailsText = product.StoryDetailsText,
                ProcessImageUrl1 = product.ProcessImageUrl1,
                ProcessImageUrl2 = product.ProcessImageUrl2,
                IsActive = product.IsActive
            };
            await PopulateCategoriesDropDownListAsync(product.CategoryId);
            return View(viewModel);
        }

        // POST: ArtisanProducts/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductViewModel model)
        {
            if (id != model.Id) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ArtisanProfileId == artisanProfile.Id);
            if (productToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    productToUpdate.Name = model.Name;
                    productToUpdate.Description = model.Description;
                    productToUpdate.Price = model.Price;
                    productToUpdate.StockQuantity = model.StockQuantity;
                    productToUpdate.CategoryId = model.CategoryId == 0 ? null : model.CategoryId;
                    productToUpdate.MainImageUrl = model.MainImageUrl;
                    productToUpdate.StoryDetailsText = model.StoryDetailsText;
                    productToUpdate.ProcessImageUrl1 = model.ProcessImageUrl1;
                    productToUpdate.ProcessImageUrl2 = model.ProcessImageUrl2;
                    productToUpdate.IsActive = model.IsActive;
                    productToUpdate.LastUpdated = DateTime.UtcNow;

                    _context.Update(productToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Products.AnyAsync(e => e.Id == productToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoriesDropDownListAsync(model.CategoryId);
            return View(model);
        }

        // GET: ArtisanProducts/DeleteProduct/5
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var product = await _context.Products
                .Include(p => p.Category) // For display on confirmation page
                .FirstOrDefaultAsync(m => m.Id == id && m.ArtisanProfileId == artisanProfile.Id);
            
            if (product == null) return NotFound();

            return View(product); 
        }

        // POST: ArtisanProducts/DeleteProduct/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ArtisanProfileId == artisanProfile.Id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully.";
            } 
            else 
            {
                TempData["ErrorMessage"] = "Product not found or you do not have permission to delete it.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- MATERIAL MANAGEMENT ---

        // GET: ArtisanProducts/CreateMaterial
        public async Task<IActionResult> CreateMaterial()
        {
            await PopulateCategoriesDropDownListAsync();
            var model = new MaterialViewModel { IsActive = true };
            return View(model);
        }

        // POST: ArtisanProducts/CreateMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMaterial(MaterialViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) 
            {
                 ModelState.AddModelError("", "Unable to find your artisan profile.");
                await PopulateCategoriesDropDownListAsync(model.CategoryId);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var material = new Material
                {
                    SupplierArtisanProfileId = artisanProfile.Id,
                    Name = model.Name,
                    Description = model.Description,
                    PricePerUnit = model.PricePerUnit,
                    UnitOfMeasure = model.UnitOfMeasure,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId == 0 ? null : model.CategoryId,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive,
                    DateAdded = DateTime.UtcNow
                };
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoriesDropDownListAsync(model.CategoryId);
            return View(model);
        }
        
        // GET: ArtisanProducts/EditMaterial/5
        public async Task<IActionResult> EditMaterial(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id && m.SupplierArtisanProfileId == artisanProfile.Id);
            if (material == null) return NotFound();

            var viewModel = new MaterialViewModel
            {
                Id = material.Id,
                Name = material.Name,
                Description = material.Description,
                PricePerUnit = material.PricePerUnit,
                UnitOfMeasure = material.UnitOfMeasure,
                StockQuantity = material.StockQuantity,
                CategoryId = material.CategoryId,
                ImageUrl = material.ImageUrl,
                IsActive = material.IsActive
            };
            await PopulateCategoriesDropDownListAsync(material.CategoryId);
            return View(viewModel);
        }

        // POST: ArtisanProducts/EditMaterial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, MaterialViewModel model)
        {
            if (id != model.Id) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var materialToUpdate = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id && m.SupplierArtisanProfileId == artisanProfile.Id);
            if (materialToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    materialToUpdate.Name = model.Name;
                    materialToUpdate.Description = model.Description;
                    materialToUpdate.PricePerUnit = model.PricePerUnit;
                    materialToUpdate.UnitOfMeasure = model.UnitOfMeasure;
                    materialToUpdate.StockQuantity = model.StockQuantity;
                    materialToUpdate.CategoryId = model.CategoryId == 0 ? null : model.CategoryId;
                    materialToUpdate.ImageUrl = model.ImageUrl;
                    materialToUpdate.IsActive = model.IsActive;
                    // materialToUpdate.DateAdded is not updated on edit. Add LastUpdated to Material if needed.

                    _context.Update(materialToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Material updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Materials.AnyAsync(e => e.Id == materialToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoriesDropDownListAsync(model.CategoryId);
            return View(model);
        }

        // GET: ArtisanProducts/DeleteMaterial/5
        public async Task<IActionResult> DeleteMaterial(int? id)
        {
            if (id == null) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var material = await _context.Materials
                .Include(m => m.Category) // For display
                .FirstOrDefaultAsync(m => m.Id == id && m.SupplierArtisanProfileId == artisanProfile.Id);
            
            if (material == null) return NotFound();

            return View(material);
        }

        // POST: ArtisanProducts/DeleteMaterial/5
        [HttpPost, ActionName("DeleteMaterial")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterialConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var artisanProfile = await _context.ArtisanProfiles.AsNoTracking().FirstOrDefaultAsync(ap => ap.UserId == currentUser!.Id);
            if (artisanProfile == null) return Forbid();

            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id && m.SupplierArtisanProfileId == artisanProfile.Id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Material not found or you do not have permission to delete it.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}