using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.Core.Entities;
using Artisans.Core.Enums;
using Artisans.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Artisans.Features.AdminDashboard
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ArtisansDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            ArtisansDBContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<AdminDashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalArtisans = await _context.ArtisanProfiles.CountAsync();
            ViewBag.ApprovedArtisans = await _context.ArtisanProfiles.CountAsync(ap => ap.IsApproved);
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalMaterials = await _context.Materials.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingInfluencerPosts = await _context.InfluencerPosts.CountAsync(p => !p.IsApproved);


            var allArtisanProfiles = await _context.ArtisanProfiles
                .Include(ap => ap.User)
                .ToListAsync();
            
            ViewBag.AllArtisanProfilesCount = allArtisanProfiles.Count;
            ViewBag.PendingArtisanProfilesCount = allArtisanProfiles.Count(ap => !ap.IsApproved);
            

            if (allArtisanProfiles.Count == 0)
            {
                TempData["DebugMessage"] = "No artisan profiles found in database. Try registering as an artisan first.";
            }
            else if (allArtisanProfiles.All(ap => ap.IsApproved))
            {
                TempData["DebugMessage"] = $"All {allArtisanProfiles.Count} artisan profiles are already approved.";
            }
            else
            {
                var pending = allArtisanProfiles.Where(ap => !ap.IsApproved).ToList();
                TempData["DebugMessage"] = $"Found {pending.Count} pending artisan profiles: {string.Join(", ", pending.Select(p => p.BrandName))}";
            }

            ViewBag.PendingArtisanProfiles = await _context.ArtisanProfiles
                .Include(ap => ap.User)
                .Where(ap => !ap.IsApproved)
                .ToListAsync();
            
            ViewBag.Users = await _userManager.Users.ToListAsync(); 

            ViewBag.PendingInfluencerPostsList = await _context.InfluencerPosts
                .Include(p => p.InfluencerUser)
                .Where(p => !p.IsApproved)
                .OrderByDescending(p => p.UploadTimestamp)
                .ToListAsync();

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> DebugDatabase()
        {
            var debugInfo = new List<string>();
            

            var totalUsers = await _userManager.Users.CountAsync();
            debugInfo.Add($"Total Users: {totalUsers}");
            

            var allProfiles = await _context.ArtisanProfiles.Include(ap => ap.User).ToListAsync();
            debugInfo.Add($"Total Artisan Profiles: {allProfiles.Count}");
            
            foreach (var profile in allProfiles)
            {
                debugInfo.Add($"- {profile.BrandName} (User: {profile.User?.UserName}) - IsApproved: {profile.IsApproved}");
            }
            

            var pendingProfiles = await _context.ArtisanProfiles
                .Include(ap => ap.User)
                .Where(ap => !ap.IsApproved)
                .ToListAsync();
            debugInfo.Add($"Pending Profiles: {pendingProfiles.Count}");
            
            ViewBag.DebugInfo = debugInfo;
            return View("DebugDatabase");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestArtisan()
        {
            try
            {

                var testUser = new User
                {
                    UserName = $"test_artisan_{DateTime.Now:yyyyMMddHHmmss}",
                    Email = $"test_artisan_{DateTime.Now:yyyyMMddHHmmss}@example.com",
                    CustomRole = UserRoleType.Artisan,
                    RegistrationDate = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(testUser, "TestPass123!");
                if (result.Succeeded)
                {

                    await _userManager.AddToRoleAsync(testUser, "Artisan");


                    var artisanProfile = new ArtisanProfile
                    {
                        UserId = testUser.Id,
                        BrandName = $"Test Artisan Brand {DateTime.Now:HHmmss}",
                        Bio = "This is a test artisan created for testing the approval system.",
                        IsApproved = false // Explicitly set to false for testing
                    };

                    _context.ArtisanProfiles.Add(artisanProfile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Test artisan '{artisanProfile.BrandName}' created successfully for testing approval functionality.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating test artisan: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveArtisan(int id)
        {
            _logger.LogInformation("Attempting to approve artisan profile with ID: {ArtisanProfileId}", id);
            var profile = await _context.ArtisanProfiles.FindAsync(id);
            if (profile != null)
            {
                var currentAdmin = await _userManager.GetUserAsync(User);
                profile.IsApproved = true;
                profile.ApprovedDate = DateTime.UtcNow;
                _context.Update(profile);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Artisan '{profile.BrandName}' approved by {currentAdmin?.UserName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Artisan profile not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectArtisan(int id)
        {
            _logger.LogInformation("Attempting to reject artisan profile with ID: {ArtisanProfileId}", id);
            var profile = await _context.ArtisanProfiles.Include(u => u.User).FirstOrDefaultAsync(p => p.Id == id);
            if (profile != null)
            {
                var user = profile.User;
                _context.ArtisanProfiles.Remove(profile);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Artisan '{profile.BrandName}' and associated user have been rejected and removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Artisan profile not found.";
            }
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInfluencerPost(int postId)
        {
            var post = await _context.InfluencerPosts.FindAsync(postId);
            if (post != null)
            {
                var currentAdmin = await _userManager.GetUserAsync(User);
                post.IsApproved = true;
                post.ApprovalTimestamp = DateTime.UtcNow;
                post.ApprovedByAdminUserId = currentAdmin?.Id;
                _context.Update(post);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Influencer post has been approved by {currentAdmin?.UserName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Influencer post not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInfluencerPost(int postId)
        {
            _logger.LogInformation("Attempting to reject influencer post with ID: {PostId}", postId);
            var post = await _context.InfluencerPosts.Include(p => p.InfluencerUser).FirstOrDefaultAsync(p => p.Id == postId);
            if (post != null)
            {
                try
                {
                    var postCaption = post.Caption ?? "Untitled";
                    var currentAdmin = await _userManager.GetUserAsync(User);

                    _context.InfluencerPosts.Remove(post);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Successfully rejected and removed influencer post '{PostCaption}'.", postCaption);
                    TempData["SuccessMessage"] = $"Influencer post by {post.InfluencerUser?.UserName} has been rejected and removed by {currentAdmin?.UserName}.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while rejecting influencer post ID: {PostId}", postId);
                    TempData["ErrorMessage"] = $"An unexpected error occurred while rejecting the post: {ex.Message}";
                }
            }
            else
            {
                _logger.LogWarning("Could not find influencer post with ID: {PostId} for rejection.", postId);
                TempData["ErrorMessage"] = "Influencer post not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(int userId, UserRoleType newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var currentAdmin = await _userManager.GetUserAsync(User);
                var oldRole = user.CustomRole;
                

                var oldRoleName = oldRole.ToString();
                if (await _userManager.IsInRoleAsync(user, oldRoleName))
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRoleName);
                }
                

                var newRoleName = newRole.ToString();
                if (await _roleManager.RoleExistsAsync(newRoleName))
                {
                    await _userManager.AddToRoleAsync(user, newRoleName);
                    user.CustomRole = newRole;
                    await _userManager.UpdateAsync(user);
                    
                    TempData["SuccessMessage"] = $"User '{user.UserName}' role changed from {oldRole} to {newRole} by {currentAdmin?.UserName}.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Role '{newRoleName}' does not exist.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var currentAdmin = await _userManager.GetUserAsync(User);
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                
                var status = user.IsActive ? "activated" : "deactivated";
                TempData["SuccessMessage"] = $"User '{user.UserName}' has been {status} by {currentAdmin?.UserName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}