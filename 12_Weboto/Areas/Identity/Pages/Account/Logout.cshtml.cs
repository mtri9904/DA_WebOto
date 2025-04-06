    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    #nullable disable

    using System;
    using System.Threading.Tasks;
    using _12_Weboto.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    namespace _12_Weboto.Areas.Identity.Pages.Account
    {
        [AllowAnonymous]
        public class LogoutModel : PageModel
        {
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly ILogger<LogoutModel> _logger;

            public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
            {
                _signInManager = signInManager;
                _logger = logger;
            }

            public async Task<IActionResult> OnPost(string returnUrl = null)
            {
                // Không xử lý nếu yêu cầu đến từ AdminController
                if (HttpContext.Request.Path.StartsWithSegments("/Admin"))
                {
                    return new EmptyResult(); // Bỏ qua Razor Page
                }
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");
                // Kiểm tra vai trò của người dùng
                if (User.IsInRole(SD.Role_Admin))
                {
                    return LocalRedirect("/Admin/Admin/Login"); // Chuyển hướng cho admin
                }

                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    return RedirectToPage("/Index");
                }
            }
        }
    }
