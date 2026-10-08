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
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrdersController(ApplicationDbContext context,
                                     UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(OrderStatus? status, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => !status.HasValue || o.Status == status.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderListItemViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    CustomerName = (o.User.FirstName + " " + o.User.LastName).Trim(),
                    Phone = o.Phone,
                    ItemsCount = o.OrderItems.Count,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                })
                .ToListAsync();

            var model = new AdminOrderListViewModel
            {
                Orders = orders,
                StatusFilter = status,
                Page = page,
                TotalPages = totalPages
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new AdminOrderDetailsViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    CustomerName = (o.User.FirstName + " " + o.User.LastName).Trim(),
                    CustomerEmail = o.User.Email ?? string.Empty,
                    Phone = o.Phone,
                    DeliveryAddress = o.DeliveryAddress,
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
                        .Select(h => new StatusHistoryItemViewModel
                        {
                            Status = h.Status,
                            ChangedAt = h.ChangedAt,
                            ChangedByName = (h.ChangedBy.FirstName + " " + h.ChangedBy.LastName).Trim()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            order.AllowedNextStatuses = OrderRules.GetAllowedNextStatuses(order.Status).ToList();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);   // tracked: we will modify it
            if (order == null) return NotFound();

            if (!OrderRules.CanTransition(order.Status, newStatus))
            {
                TempData["Error"] = $"Cannot move an order from {order.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = newStatus;
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = _userManager.GetUserId(User)!
            });

            await _context.SaveChangesAsync();   // status + history in ONE transaction

            TempData["Success"] = $"Order #{order.Id} marked as {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
