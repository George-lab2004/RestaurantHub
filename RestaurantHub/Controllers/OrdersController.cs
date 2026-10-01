using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantHub.Helpers;
using RestaurantHub.Models;
using RestaurantHub.Services;
using RestaurantHub.ViewModel;

namespace RestaurantHub.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cart;

        public OrdersController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                ICartService cart)
        {
            _context = context;
            _userManager = userManager;
            _cart = cart;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await _cart.BuildAsync();
            if (cart.IsEmpty)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Menu");
            }

            var user = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                DeliveryAddress = user?.Address ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                PaymentMethod = OrderRules.PaymentMethods[0],
                Cart = cart
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = await _cart.BuildAsync();   // always rebuilt on the server
            if (cart.IsEmpty)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Menu");
            }

            if (!OrderRules.PaymentMethods.Contains(model.PaymentMethod))
                ModelState.AddModelError(nameof(model.PaymentMethod), "Please choose a valid payment method.");

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View(model);
            }

            var order = new Order
            {
                UserId = _userManager.GetUserId(User)!,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                DeliveryAddress = model.DeliveryAddress.Trim(),
                Phone = model.Phone.Trim(),
                Notes = model.Notes?.Trim(),
                PaymentMethod = model.PaymentMethod,
                Subtotal = cart.Subtotal,
                Tax = cart.Tax,
                TotalPrice = cart.Total,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    ItemName = i.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _cart.Clear();

            // Section 15: this is where we'll notify admins through SignalR.

            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new OrderDetailsViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    DeliveryAddress = o.DeliveryAddress,
                    Phone = o.Phone,
                    PaymentMethod = o.PaymentMethod,
                    Notes = o.Notes,
                    Subtotal = o.Subtotal,
                    Tax = o.Tax,
                    TotalPrice = o.TotalPrice,
                    Lines = o.OrderItems.Select(oi => new OrderLineViewModel
                    {
                        ItemName = oi.ItemName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return View(order);
        }
    }
}