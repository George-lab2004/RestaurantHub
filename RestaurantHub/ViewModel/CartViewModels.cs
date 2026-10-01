using RestaurantHub.Helpers;

namespace RestaurantHub.ViewModel
{
    // What we store in the session: ids and quantities only. No prices.
    public class CartLine
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    // What we show on screen: built from the database every time.
    public class CartItemViewModel
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CartViewModels
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public bool IsEmpty => Items.Count == 0;
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal Tax => OrderRules.CalculateTax(Subtotal);
        public decimal Total => Subtotal + Tax;
    }
}