using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Infrastructure.Persistance.Seeding
{
    public static class SeedingExtension
    {
        public static async Task ApplyMigrationsAndSeedAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var context = scopedServices.GetRequiredService<AppDbContext>();
            var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync(); 
            await SeedingManager.SeedAsync(userManager, roleManager);
        }
    }
}
