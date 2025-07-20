using System.ComponentModel.DataAnnotations;
using Artisans.Models; 

namespace Artisans.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Register as")]
        public UserRoleType Role { get; set; } = UserRoleType.Buyer;

        
        [Display(Name = "Brand Name (if registering as Artisan)")]
        [StringLength(100)]
        public string? BrandName { get; set; }

        [Display(Name = "Short Bio (if registering as Artisan)")]
        [StringLength(500)]
        public string? Bio { get; set; }
    }
}