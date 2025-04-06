using System.Text;
using System.Threading.Tasks;
using _12_Weboto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace _12_Weboto.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [BindProperty]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl = null)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{userId}'.");   
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                // Tự động đăng nhập sau khi xác nhận thành công
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            else
            {
                StatusMessage = "Liên kết xác nhận không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu gửi lại email xác nhận.";
                return Page();
            }
        }
    }
}