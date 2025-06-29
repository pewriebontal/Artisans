
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.Core.Entities;
using Artisans.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Artisans.Features.AdminDashboard
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ArtisansDBContext _context;
        private readonly UserManager<User> _userManager;

        public AdminDashboardController(ArtisansDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PendingArtisanProfiles = await _context.ArtisanProfiles
                .Include(ap => ap.User)
                .Where(ap => !ap.IsApproved)
                .ToListAsync();
            
            ViewBag.Users = await _userManager.Users.ToListAsync(); 

            ViewBag.PendingInfluencerPosts = await _context.InfluencerPosts
                .Include(p => p.InfluencerUser)
                .Where(p => !p.IsApproved)
                .OrderByDescending(p => p.UploadTimestamp)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveArtisan(int artisanProfileId)
        {
            var profile = await _context.ArtisanProfiles.FindAsync(artisanProfileId);
            if (profile != null)
            {
                profile.IsApproved = true;
                profile.ApprovedDate = DateTime.UtcNow;
                _context.Update(profile);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Artisan {profile.BrandName} approved.";
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
                post.IsApproved = true;
                post.ApprovalTimestamp = DateTime.UtcNow;
                // post.ApprovedByAdminUserId = (await _userManager.GetUserAsync(User)).Id; // If tracking admin
                _context.Update(post);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Influencer post approved.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Add actions for RejectArtisan, RejectInfluencerPost, ManageUserRoles etc. as needed/time permits
    }
}