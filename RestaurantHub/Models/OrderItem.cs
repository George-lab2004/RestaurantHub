using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantHub.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;
        public string ItemName { get; set; } = string.Empty;   // frozen at order time
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // frozen at order time
    }
}
