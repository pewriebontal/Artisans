using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Artisans.Core.Entities;
using Artisans.Core.Enums;
using Artisans.Features.Account.ViewModels;
using System.Threading.Tasks;
using Artisans.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; 

namespace Artisans.Features.Account
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ArtisansDBContext _context;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            ArtisansDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    CustomRole = model.Role,
                    RegistrationDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string roleName = model.Role.ToString();
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Role {roleName} does not exist.");
                        return View(model);
                    }

                    if (model.Role == UserRoleType.Artisan)
                    {
                        if (string.IsNullOrWhiteSpace(model.BrandName))
                        {
                            ModelState.AddModelError(nameof(model.BrandName), "Brand Name is required for Artisans.");
                            // Consider deleting the user if profile creation fails and is mandatory
                            // await _userManager.DeleteAsync(user); 
                            return View(model);
                        }
                        var artisanProfile = new ArtisanProfile
                        {
                            UserId = user.Id,
                            BrandName = model.BrandName,
                            Bio = model.Bio,
                            IsApproved = false
                        };
                        _context.ArtisanProfiles.Add(artisanProfile);
                        await _context.SaveChangesAsync();
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

                if (user != null && user.IsActive)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            // We don't have an Admin area/Dashboard controller yet, so redirect to Home for now
                            return RedirectToAction("Index", "Home", new { area = "" }); // <<< Placeholder
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Artisan"))
                        {
                            var artisanProfile = await _context.ArtisanProfiles.FirstOrDefaultAsync(ap => ap.UserId == user.Id);
                            if (artisanProfile != null && artisanProfile.IsApproved)
                            {
                                // We don't have ArtisanProducts controller yet, so redirect to Home for now
                                return RedirectToAction("Index", "Home", new { area = "" }); // <<< Placeholder
                            }
                            else
                            {
                                await _signInManager.SignOutAsync();
                                ModelState.AddModelError(string.Empty, "Your artisan account is pending approval or setup is incomplete.");
                                return View(model);
                            }
                        }
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" }); // <<<< CORRECTED
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }
        #endregion
    }
}