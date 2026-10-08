namespace RestaurantHub.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus Status { get; set; }              // the enum itself, no "StatusId"

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string ChangedByUserId { get; set; } = string.Empty;
        public ApplicationUser ChangedBy { get; set; } = null!;
    }
}