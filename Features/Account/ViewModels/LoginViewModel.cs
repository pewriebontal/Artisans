using System.ComponentModel.DataAnnotations;

namespace Artisans.Features.Account.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username or Email")] 
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}