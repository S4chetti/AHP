namespace AHP.Models
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    namespace CoreApiProject.Models
    {
        public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
            {
            }

            public DbSet<Survey> Surveys { get; set; }
            public DbSet<Question> Questions { get; set; }
            public DbSet<Option> Options { get; set; }
            public DbSet<Category> Categories { get; set; }
        }
    }
}
