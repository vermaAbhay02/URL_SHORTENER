using Microsoft.AspNetCore.Identity;
using URL_Shortener.Models;

namespace URL_Shortener.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Create Admin user if it doesn't exist
        var adminEmail = "admin@urlshortener.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = adminEmail,
                UserName = adminEmail,
                Plan = Plan.Pro
            };

            await userManager.CreateAsync(adminUser, "Admin@1234");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}