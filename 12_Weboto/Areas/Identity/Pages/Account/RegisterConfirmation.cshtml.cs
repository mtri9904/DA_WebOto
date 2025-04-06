using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _12_Weboto.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; }
        public string ReturnUrl { get; set; }

        public void OnGet(string email, string returnUrl = null)
        {
            Email = email;
            ReturnUrl = returnUrl;
        }
    }
}