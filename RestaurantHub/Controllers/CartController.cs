using Microsoft.AspNetCore.Mvc;
using RestaurantHub.Services;

namespace RestaurantHub.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        public async Task<IActionResult> Index() => View(await _cart.BuildAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int menuItemId, int quantity = 1)
        {
            var added = await _cart.AddAsync(menuItemId, quantity);

            if (added) TempData["Success"] = "Added to your cart.";
            else TempData["Error"] = "Sorry, this item is no longer available.";

            return RedirectToAction("Index", "Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int menuItemId, int quantity)
        {
            _cart.SetQuantity(menuItemId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int menuItemId)
        {
            _cart.Remove(menuItemId);
            return RedirectToAction(nameof(Index));
        }
    }
}