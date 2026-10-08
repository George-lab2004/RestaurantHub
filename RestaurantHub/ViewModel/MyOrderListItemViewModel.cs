using RestaurantHub.Models;

namespace RestaurantHub.ViewModel
{
    public class MyOrderListItemViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
    }
}