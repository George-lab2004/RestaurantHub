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

        // ───────── MY ORDERS ─────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)                  // rule 9: only MY orders
                .OrderByDescending(o => o.CreatedAt)             // newest first
                .Select(o => new MyOrderListItemViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    ItemsCount = o.OrderItems.Count,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                })
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.UserId == userId)    // id AND owner → otherwise 404 (IDOR protection)
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
                    }).ToList(),
                    History = o.StatusHistory
                        .OrderBy(h => h.ChangedAt)
                        .Select(h => new StatusHistoryItemViewModel   // no staff names for customers
                        {
                            Status = h.Status,
                            ChangedAt = h.ChangedAt
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            order.CanCancel = OrderRules.CanCustomerCancel(order.Status);   // computed from the rule, not stored
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var order = await _context.Orders                      // tracked: we are going to modify it
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);   // mine only
            if (order == null) return NotFound();

            if (!OrderRules.CanCustomerCancel(order.Status))        // the admin may have confirmed it a second ago
            {
                TempData["Error"] = "This order can no longer be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = OrderStatus.Cancelled;
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Cancelled,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId
            });

            await _context.SaveChangesAsync();                      // status + history in ONE transaction

            // Section 15: notify admins here (SignalR), AFTER the save
            TempData["Success"] = "Your order was cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ───────── CHECKOUT ─────────
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
            var cart = await _cart.BuildAsync();
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

            var userId = _userManager.GetUserId(User)!;   // read once, reuse everywhere below
            var now = DateTime.UtcNow;                    // one timestamp for the order and its first history row

            var order = new Order
            {
                UserId = userId,
                CreatedAt = now,
                Status = OrderStatus.Pending,
                DeliveryAddress = model.DeliveryAddress.Trim(),
                Phone = model.Phone.Trim(),
                Notes = model.Notes?.Trim(),
                PaymentMethod = model.PaymentMethod,
                Subtotal = cart.Subtotal,
                Tax = cart.Tax,
                TotalPrice = cart.Total,
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        Status = OrderStatus.Pending,
                        ChangedAt = now,
                        ChangedByUserId = userId          // ← THE FIX: a real user id, not ""
                    }
                },
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    ItemName = i.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();            // order + items + first history row, one transaction

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