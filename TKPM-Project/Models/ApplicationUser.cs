﻿using Microsoft.AspNetCore.Identity;

namespace TKPM_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}