using Microsoft.AspNetCore.Identity;
using NewsSite.Models.Entities;

namespace NewsSite.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Writer", "Editor", "Subscriber", "Reader" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@newssite.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            await SeedWritersAsync(userManager);
        }
        private static async Task SeedWritersAsync(UserManager<ApplicationUser> userManager)
        {
            var writers = new (string Email, string FirstName, string LastName, string Password)[]
            {
        ("anna.bergman@newsite.com", "Anna", "Bergman", "Writer123!"),
        ("erik.lindgren@newsite.com", "Erik", "Lindgren", "Writer123!"),
        ("sara.wallin@newsite.com", "Sara", "Wallin", "Writer123!"),
        ("mikael.sundstrom@newsite.com", "Mikael", "Sundström", "Writer123!")
            };

            foreach (var writer in writers)
            {
                var existingUser = await userManager.FindByEmailAsync(writer.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = writer.Email,
                        Email = writer.Email,
                        FirstName = writer.FirstName,
                        LastName = writer.LastName,
                        EmailConfirmed = true 
                    };

                    var result = await userManager.CreateAsync(user, writer.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Writer");
                    }
                }
            }
        }
    }
}