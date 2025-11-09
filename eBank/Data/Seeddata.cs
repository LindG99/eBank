using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace eBank.Data
{
    // Seed initial data for roles and users
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // create "User" role if it doesn't exist
            string roleName = "User";
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // assign existing user to "User" role
            string existingEmail = "gustav.lind.99@live.se"; 
            var existingUser = await userManager.FindByEmailAsync(existingEmail);
            if (existingUser != null && !await userManager.IsInRoleAsync(existingUser, roleName))
            {
                await userManager.AddToRoleAsync(existingUser, roleName);
            }
        }
    }
}

