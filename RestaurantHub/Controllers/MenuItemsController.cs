using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantHub.Models;
using RestaurantHub.Services;
using RestaurantHub.ViewModel;

namespace RestaurantHub.Controllers
{
    // Only admin can manage menu items
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        //admin will need access the database and file service to manage menu items
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        //now we make constructor to inject the dependencies of ApplicationDbContext and IFileService into the controller
        public MenuItemsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }
        public async Task<IActionResult> Index()
        {
            var items = await _context.MenuItems
                .AsNoTracking() //so that we don't track the changes to the entities in the context, which can improve performance for read-only scenarios
                .OrderBy(m => m.Category.Name)
                .ThenBy(m => m.Name)
                .Select(m => new MenuItemListItemViewModel // we can choose what data we want to return to the view by projecting the MenuItem entity into a MenuItemListItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    CategoryName = m.Category.Name,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();

            return View(items);
        }
        //to make a new item we need get for the form and post the form data to create a new item in the database
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new MenuItemViewModel(); //we create a new instance of the MenuItemViewModel class to hold the data for the new menu item
            await PopulateCategoriesAsync(model); //populate means to fill the Categories property of the model with a list of categories from the database, so that the user can select a category for the new menu item
            return View("Form", model);
        }
        // first we need check data is valid according to attributes we put in view model then we need see if id valid, then make sure no duplicate name in the same category, then we need to save the image file if there is one, then we need to create a new MenuItem entity and save it to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItemViewModel model)
        {
            if (!await IsValidCategoryAsync(model)) { }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model);
                return View("Form", model);
            }

            var item = new MenuItem
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Price = model.Price,
                CategoryId = model.CategoryId,
                IsAvailable = model.IsAvailable
            };

            if (model.ImageFile is { Length: > 0 })
            {
                try
                {
                    item.ImageUrl = await _fileService.SaveImageAsync(model.ImageFile, "menu");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    await PopulateCategoriesAsync(model);
                    return View("Form", model);
                }
            }

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Menu item created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

            var model = new MenuItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CategoryId = item.CategoryId,
                IsAvailable = item.IsAvailable,
                ExistingImageUrl = item.ImageUrl
            };

            await PopulateCategoriesAsync(model);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuItemViewModel model)
        {
            await IsValidCategoryAsync(model);

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model);
                return View("Form", model);
            }

            var item = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == model.Id);
            if (item == null) return NotFound();

            string? oldImage = null;

            if (model.ImageFile is { Length: > 0 })
            {
                try
                {
                    var newPath = await _fileService.SaveImageAsync(model.ImageFile, "menu");
                    oldImage = item.ImageUrl;
                    item.ImageUrl = newPath;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    await PopulateCategoriesAsync(model);
                    return View("Form", model);
                }
            }

            item.Name = model.Name.Trim();
            item.Description = model.Description?.Trim();
            item.Price = model.Price;
            item.CategoryId = model.CategoryId;
            item.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();
            _fileService.DeleteImage(oldImage);

            TempData["Success"] = "Menu item updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            item.IsAvailable = !item.IsAvailable;
            await _context.SaveChangesAsync();

            TempData["Success"] = item.IsAvailable
                ? $"\"{item.Name}\" is now available."
                : $"\"{item.Name}\" is now hidden from the menu.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(MenuItemViewModel model)
        {
            model.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToListAsync();
        }

        private async Task<bool> IsValidCategoryAsync(MenuItemViewModel model)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!exists)
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            return exists;
        }
    }
}
