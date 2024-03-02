using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using static Common.Enums;

namespace Services.DataInitializer
{
    public class UserDataInitializer : IDataInitializer
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger<UserDataInitializer> logger;

        public UserDataInitializer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserDataInitializer> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        public void InitializeData()
        {
            logger.LogInformation("start seeding");
            if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole { Name = "Admin" }).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole { Name = "User" }).GetAwaiter().GetResult();
            }
            if (!userManager.Users.AsNoTracking().Any(p => p.UserName == "Admin"))
            {
                var user = new User
                {
                    Age = 30,
                    FullName = "Admin Admin",
                    Gender = GenderType.Male,
                    UserName = "admin",
                    Email = "admin@site.com",
                    IsActive = true,
                };
                userManager.CreateAsync(user, "Admin@123").GetAwaiter().GetResult();
                userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
            }
        }
    }
}