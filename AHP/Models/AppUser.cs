namespace AHP.Models
{
    using Microsoft.AspNetCore.Identity;
    using System;

    namespace CoreApiProject.Models
    {
        public class AppUser : IdentityUser
        {
            public string? FullName { get; set; }
            public DateTime RegistrationDate { get; set; } = DateTime.Now;
        }
    }
}