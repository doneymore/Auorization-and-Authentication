using AuthenticationLinkdien.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationLinkdien.Seeding
{
    public class AppDbSeeding
    {
        public static async Task SeedRoleToDb(IApplicationBuilder appBuilder)
        {
            using (var sericeScope = appBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = sericeScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(SeedFile.Manager))
                    await roleManager.CreateAsync(new IdentityRole(SeedFile.Manager));

                if (!await roleManager.RoleExistsAsync(SeedFile.Student))
                    await roleManager.CreateAsync(new IdentityRole(SeedFile.Student));
            }
        }
    }
}
