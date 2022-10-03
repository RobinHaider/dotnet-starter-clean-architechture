using System.Security.Claims;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class Seed
    {
        public static async Task SeedData(DataContext context, UserManager<AppUser> userManager)
        {

            // seed activities
            if (!context.Activities.Any())
            {
                var activities = new List<Activity>
                {
                    new Activity
                    {
                        Title = "Past Activity 1",
                        Date = DateTime.Now.AddMonths(-2),
                        Description = "Activity 2 months ago",
                        Category = "drinks",
                        City = "London",
                        Venue = "Pub",
                    },
                    new Activity
                    {
                        Title = "Past Activity 2",
                        Date = DateTime.Now.AddMonths(-1),
                        Description = "Activity 1 month ago",
                        Category = "culture",
                        City = "Paris",
                        Venue = "Louvre",
                    },
                    new Activity
                    {
                        Title = "Future Activity 1",
                        Date = DateTime.Now.AddMonths(1),
                        Description = "Activity 1 month in future",
                        Category = "culture",
                        City = "London",
                        Venue = "Natural History Museum",
                    },
                    new Activity
                    {
                        Title = "Future Activity 2",
                        Date = DateTime.Now.AddMonths(2),
                        Description = "Activity 2 months in future",
                        Category = "music",
                        City = "London",
                        Venue = "O2 Arena",
                    },
                    new Activity
                    {
                        Title = "Future Activity 3",
                        Date = DateTime.Now.AddMonths(3),
                        Description = "Activity 3 months in future",
                        Category = "drinks",
                        City = "London",
                        Venue = "Another pub",
                    },
                    new Activity
                    {
                        Title = "Future Activity 4",
                        Date = DateTime.Now.AddMonths(4),
                        Description = "Activity 4 months in future",
                        Category = "drinks",
                        City = "London",
                        Venue = "Yet another pub",
                    },
                    new Activity
                    {
                        Title = "Future Activity 5",
                        Date = DateTime.Now.AddMonths(5),
                        Description = "Activity 5 months in future",
                        Category = "drinks",
                        City = "London",
                        Venue = "Just another pub",
                    },
                    new Activity
                    {
                        Title = "Future Activity 6",
                        Date = DateTime.Now.AddMonths(6),
                        Description = "Activity 6 months in future",
                        Category = "music",
                        City = "London",
                        Venue = "Roundhouse Camden",
                    },
                    new Activity
                    {
                        Title = "Future Activity 7",
                        Date = DateTime.Now.AddMonths(7),
                        Description = "Activity 2 months ago",
                        Category = "travel",
                        City = "London",
                        Venue = "Somewhere on the Thames",
                    },
                    new Activity
                    {
                        Title = "Future Activity 8",
                        Date = DateTime.Now.AddMonths(8),
                        Description = "Activity 8 months in future",
                        Category = "film",
                        City = "London",
                        Venue = "Cinema",
                    }
                };

                await context.Activities.AddRangeAsync(activities);
                await context.SaveChangesAsync();
            }
        }

        public static async Task IdenityData(DataContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // add roles.......
            var roles = new List<IdentityRole>
            {
                new IdentityRole{Name = "superadmin"},
                new IdentityRole{Name = "admin"},
                new IdentityRole{Name = "adminuser"},
                new IdentityRole{Name = "customer"},

            };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
                    await roleManager.CreateAsync(role);
            }

            //add admin user claims...
            var adminUserRole = await roleManager.FindByNameAsync("adminuser");

            var adminUserClaims = new List<Claim>
            {
                // new Claim("ProductModulePermission", "Delete"),
                //add admin user claims
                new Claim("UserPermission", "ReadOnly"),
                new Claim("UserPermission", "Add"),
                new Claim("UserPermission", "Update"),
                new Claim("UserPermission", "Delete"),
            };

            foreach (var claim in adminUserClaims)
            {
                if (!context.RoleClaims.AnyAsync(r => r.RoleId == adminUserRole.Id && r.ClaimType == claim.Type && r.ClaimValue == claim.Value).GetAwaiter().GetResult())
                    await roleManager.AddClaimAsync(adminUserRole, claim);
            }


            // add user
            if (!userManager.Users.Any())
            {
                var superAdmin = new AppUser
                {
                    Email = "superadmin@gmail.com",
                    UserName = "SuperAdminUser"
                };
                await userManager.CreateAsync(superAdmin, "123456");
                await userManager.AddToRoleAsync(superAdmin, "superadmin");

                var admin = new AppUser
                {
                    Email = "admin@gmail.com",
                    UserName = "AdminUser"
                };
                await userManager.CreateAsync(admin, "123456");
                await userManager.AddToRoleAsync(admin, "admin");

                var customer = new AppUser
                {
                    Email = "customer@gmail.com",
                    UserName = "CustomerUser"
                };
                await userManager.CreateAsync(customer, "123456");
                await userManager.AddToRoleAsync(customer, "customer");

                // seed users

                var users = new List<AppUser>{
                    new AppUser {DisplayName="Bob", UserName="bob", Email="bob@test.com"},
                    new AppUser {DisplayName="Tom", UserName="tom", Email="tom@test.com"},
                    new AppUser {DisplayName="Jane", UserName="jane", Email="jane@test.com"}
                };

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Password");
                    await userManager.AddToRoleAsync(customer, "customer");

                }



            }
        }
    }
}
