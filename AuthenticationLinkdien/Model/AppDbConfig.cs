using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationLinkdien.Model
{
    public class AppDbConfig : IdentityDbContext<ApplicationUser>
    {
        public AppDbConfig(DbContextOptions<AppDbConfig> options) : base(options)
        {

        }

        public DbSet<User1> Users { get; set; }
        public DbSet<RefreshToken> RefToken { get; set; }

        internal Task FndByIdAsync(object storedToken)
        {
            throw new NotImplementedException();
        }
    }
}
