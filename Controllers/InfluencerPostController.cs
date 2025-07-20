
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Artisans.Models;
using Artisans.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Artisans.Controllers
{
    [Authorize(Roles = "Influencer,Admin")] 
    public class InfluencerPostController : Controller
    {
        private readonly ArtisansDBContext _context;
        private readonly UserManager<User> _userManager;

        public InfluencerPostController(ArtisansDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        private async Task PopulateArtisanProfilesDropDownListAsync(object? selectedProfile1 = null, object? selectedProfile2 = null)
        {
            var artisanProfilesQuery = from ap in _context.ArtisanProfiles
                                       where ap.IsApproved 
                                       orderby ap.BrandName
                                       select new { ap.Id, ap.BrandName };
            
            ViewBag.AvailableArtisanProfiles = new SelectList(await artisanProfilesQuery.AsNoTracking().ToListAsync(), "Id", "BrandName");
        }


        
        public async Task<IActionResult> MyPosts()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var posts = await _context.InfluencerPosts
                .Include(p => p.Tags)
                    .ThenInclude(t => t.TaggedArtisanProfile)
                .Where(p => p.InfluencerUserId == currentUser.Id)
                .OrderByDescending(p => p.UploadTimestamp)
                .ToListAsync();
            return View(posts);
        }


        
        public async Task<IActionResult> Create()
        {
            await PopulateArtisanProfilesDropDownListAsync();
            return View(new CreatePostViewModel());
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (ModelState.IsValid)
            {
                var post = new InfluencerPost
                {
                    InfluencerUserId = currentUser.Id,
                    ImageUrl = model.ImageUrl,
                    Caption = model.Caption,
                    UploadTimestamp = DateTime.UtcNow,
                    IsApproved = false 
                };

                
                post.Tags = new List<PostTag>();
                if (model.TaggedArtisanProfileId1.HasValue)
                {
                    post.Tags.Add(new PostTag { TaggedArtisanProfileId = model.TaggedArtisanProfileId1.Value });
                }
                if (model.TaggedArtisanProfileId2.HasValue && model.TaggedArtisanProfileId2 != model.TaggedArtisanProfileId1) 
                {
                    post.Tags.Add(new PostTag { TaggedArtisanProfileId = model.TaggedArtisanProfileId2.Value });
                }

                _context.InfluencerPosts.Add(post);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Post submitted for approval!";
                return RedirectToAction(nameof(MyPosts)); 
            }
            await PopulateArtisanProfilesDropDownListAsync();
            return View(model);
        }

        
        
        
    }
}