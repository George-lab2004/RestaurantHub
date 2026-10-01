using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantHub.Models;
using RestaurantHub.Services;
using RestaurantHub.ViewModel;

namespace RestaurantHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public CategoriesController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                    ItemsCount = c.MenuItems.Count
                })
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create() => View("Form", new CategoryViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View("Form", model);

            var name = model.Name.Trim();

            if (await _context.Categories.AnyAsync(c => c.Name == name))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View("Form", model);
            }

            var category = new Category { Name = name };

            if (model.ImageFile is { Length: > 0 })
            {
                try
                {
                    category.ImageUrl = await _fileService.SaveImageAsync(model.ImageFile, "categories");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    return View("Form", model);
                }
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View("Form", new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ExistingImageUrl = category.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View("Form", model);

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (category == null) return NotFound();

            var name = model.Name.Trim();

            if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View("Form", model);
            }

            string? oldImage = null;

            if (model.ImageFile is { Length: > 0 })
            {
                try
                {
                    var newPath = await _fileService.SaveImageAsync(model.ImageFile, "categories");
                    oldImage = category.ImageUrl;
                    category.ImageUrl = newPath;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                    return View("Form", model);
                }
            }

            category.Name = name;
            await _context.SaveChangesAsync();

            _fileService.DeleteImage(oldImage);

            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            if (await _context.MenuItems.AnyAsync(m => m.CategoryId == id))
            {
                TempData["Error"] = "You can't delete a category that still has menu items.";
                return RedirectToAction(nameof(Index));
            }

            var imagePath = category.ImageUrl;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _fileService.DeleteImage(imagePath);

            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}