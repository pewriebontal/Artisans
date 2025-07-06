using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.ViewModels;
using Artisans.Infrastructure.Data;

namespace Artisans.Features.Home;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ArtisansDBContext _context;

    public HomeController(ILogger<HomeController> logger, ArtisansDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {

        var featuredProducts = await _context.Products
            .Include(p => p.ArtisanProfile)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.ArtisanProfile.IsApproved)
            .OrderByDescending(p => p.DateAdded)
            .Take(6)
            .ToListAsync();

        return View(featuredProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
