
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Artisans.Controllers
{
    public class InfluencerFeedController : Controller
    {
        private readonly ArtisansDBContext _context;

        public InfluencerFeedController(ArtisansDBContext context)
        {
            _context = context;
        }

        
        [Route("ForYou")] 
        public async Task<IActionResult> Index()
        {
            var posts = await _context.InfluencerPosts
                .Include(p => p.InfluencerUser) 
                .Include(p => p.Tags)
                    .ThenInclude(t => t.TaggedArtisanProfile) 
                .Where(p => p.IsApproved)
                .OrderByDescending(p => p.UploadTimestamp)
                .Take(20) 
                .ToListAsync();
            return View(posts);
        }
    }
}