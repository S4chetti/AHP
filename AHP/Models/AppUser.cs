namespace AHP.Models
{
    using Microsoft.AspNetCore.Identity;

    namespace CoreApiProject.Models
    {
        public class AppUser : IdentityUser
        {
            public string FullName { get; set; }
        }
    }
}
