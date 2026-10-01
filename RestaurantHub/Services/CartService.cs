using Microsoft.EntityFrameworkCore;
using RestaurantHub.Helpers;
using RestaurantHub.Models;
using RestaurantHub.ViewModel;

namespace RestaurantHub.Services
{
    public class CartService : ICartService
    {
        private const string CartKey = "Cart";
        private const int MaxQuantityPerItem = 20;

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;

        public CartService(ApplicationDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        private List<CartLine> GetLines() => Session.GetObject<List<CartLine>>(CartKey) ?? new();

        private void SaveLines(List<CartLine> lines) => Session.SetObject(CartKey, lines);

        public async Task<bool> AddAsync(int menuItemId, int quantity)
        {
            quantity = Math.Clamp(quantity, 1, MaxQuantityPerItem);

            var available = await _context.MenuItems
                .AnyAsync(m => m.Id == menuItemId && m.IsAvailable);
            if (!available) return false;

            var lines = GetLines();
            var line = lines.FirstOrDefault(l => l.MenuItemId == menuItemId);

            if (line == null)
                lines.Add(new CartLine { MenuItemId = menuItemId, Quantity = quantity });
            else
                line.Quantity = Math.Min(line.Quantity + quantity, MaxQuantityPerItem);

            SaveLines(lines);
            return true;
        }

        public void SetQuantity(int menuItemId, int quantity)
        {
            var lines = GetLines();
            var line = lines.FirstOrDefault(l => l.MenuItemId == menuItemId);
            if (line == null) return;

            if (quantity <= 0) lines.Remove(line);
            else line.Quantity = Math.Min(quantity, MaxQuantityPerItem);

            SaveLines(lines);
        }

        public void Remove(int menuItemId)
        {
            var lines = GetLines();
            lines.RemoveAll(l => l.MenuItemId == menuItemId);
            SaveLines(lines);
        }

        public void Clear() => Session.Remove(CartKey);

        public async Task<CartViewModels> BuildAsync()
        {
            var lines = GetLines();
            var ids = lines.Select(l => l.MenuItemId).ToList();

            var items = await _context.MenuItems
                .AsNoTracking()
                .Where(m => ids.Contains(m.Id) && m.IsAvailable)
                .ToListAsync();

            var cart = new CartViewModels();

            foreach (var line in lines)
            {
                var item = items.FirstOrDefault(m => m.Id == line.MenuItemId);
                if (item == null) continue;   // hidden or removed since it was added

                cart.Items.Add(new CartItemViewModel
                {
                    MenuItemId = item.Id,
                    Name = item.Name,
                    ImageUrl = item.ImageUrl,
                    UnitPrice = item.Price,
                    Quantity = line.Quantity
                });
            }

            return cart;
        }
    }
}