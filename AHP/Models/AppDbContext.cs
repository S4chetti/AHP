using AHP.Models.CoreApiProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace AHP.Models
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
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Answer>()
                .HasOne(a => a.Option)
                .WithMany()
                .HasForeignKey(a => a.OptionId)
                .OnDelete(DeleteBehavior.Restrict);

            string adminRoleId = "41595188-4660-45f8-b3ab-d21820dd5e3b";
            string userRoleId = "76b6bbba-3f81-45ab-85fa-702330f8101a";

            builder.Entity<AppRole>().HasData(
                new AppRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new AppRole { Id = userRoleId, Name = "User", NormalizedName = "USER" }
            );

            string adminUserId = "8177bdcf-4512-4043-baeb-cd0324b94a11";
            var hasher = new PasswordHasher<AppUser>();

            builder.Entity<AppUser>().HasData(new AppUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@ahp.com",
                NormalizedEmail = "ADMIN@AHP.COM",
                FullName = "Sistem Yöneticisi",
                RegistrationDate = new DateTime(2026, 1, 1),
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Admin123*")
            });

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            });
        }
    }
}