﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace _12_Weboto.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
    }
}
