using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantHub.Models;
using RestaurantHub.ViewModel;

namespace RestaurantHub.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.MenuItems.Any(m => m.IsAvailable))
                .OrderBy(c => c.Name)
                .Select(c => new MenuCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Items = c.MenuItems
                        .Where(m => m.IsAvailable)
                        .OrderBy(m => m.Name)
                        .Select(m => new MenuItemCardViewModel
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Description = m.Description,
                            Price = m.Price,
                            ImageUrl = m.ImageUrl
                        })
                        .ToList()
                })
                .ToListAsync();

            return View(categories);
        }
    }
}