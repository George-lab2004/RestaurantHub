using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantHub.Models
{
    public enum OrderStatus { Pending, Confirmed, Preparing, Ready, Delivered, Cancelled }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public List<OrderItem> OrderItems { get; set; } = new();
        public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    }
    }
