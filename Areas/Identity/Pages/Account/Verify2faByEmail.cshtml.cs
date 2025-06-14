using _2280613193_webdocongnghe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace _2280613193_webdocongnghe.Areas.Identity.Pages.Account
{
    public class Verify2faByEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Verify2faByEmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        [Required]
        public string Code { get; set; }
        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }



        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("Login");
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return RedirectToPage("Login");
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", Code);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Mã không hợp lệ.");
                return Page();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl ?? "~/");
        }

    }
}
