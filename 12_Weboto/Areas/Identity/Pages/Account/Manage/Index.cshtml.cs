// Areas/Identity/Pages/Account/Manage/Index.cshtml.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using _12_Weboto.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _12_Weboto.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [StringLength(200, ErrorMessage = "The {0} must be at max {1} characters long.")]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }
            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
            }
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = Input.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Cập nhật Profile người dùng thành công!";
            return RedirectToPage();
        }
    }
}