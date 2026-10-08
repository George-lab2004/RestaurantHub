using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RestaurantHub.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            foreach (var role in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (!await context.Categories.AnyAsync())                            // idempotent: only when the menu is empty
            {
                var burgers = new Category { Name = "Burgers" };
                var drinks = new Category { Name = "Drinks" };
                var desserts = new Category { Name = "Desserts" };

                context.MenuItems.AddRange(
                    new MenuItem { Name = "Classic Burger", Description = "Beef patty, cheddar, lettuce, tomato", Price = 120m, Category = burgers, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&h=400&fit=crop" },
                    new MenuItem { Name = "Chicken Burger", Description = "Crispy chicken, garlic mayo", Price = 105m, Category = burgers, ImageUrl = "https://images.unsplash.com/photo-1606755962773-d324e0a13086?w=600&h=400&fit=crop" },
                    new MenuItem { Name = "Fresh Orange Juice", Description = "Squeezed to order", Price = 45m, Category = drinks, ImageUrl = "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=600&h=400&fit=crop" },
                    new MenuItem { Name = "Iced Coffee", Description = "Cold brew with milk", Price = 55m, Category = drinks, ImageUrl = "https://images.unsplash.com/photo-1517959105821-eaf2591984ca?w=600&h=400&fit=crop" },
                    new MenuItem { Name = "Chocolate Brownie", Description = "Warm, with ice cream", Price = 70m, Category = desserts, ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=600&h=400&fit=crop" });

                await context.SaveChangesAsync();                                // EF inserts the 3 categories automatically (object graph)
            }
            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    Address=""
                };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}