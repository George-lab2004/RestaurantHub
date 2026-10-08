using RestaurantHub.Models;

namespace RestaurantHub.ViewModel
{
    public class AdminOrderListItemViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
    }

    // wrapper for the list page: one page of orders + filter + paging info
    public class AdminOrderListViewModel
    {
        public List<AdminOrderListItemViewModel> Orders { get; set; } = new();
        public OrderStatus? StatusFilter { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    public class StatusHistoryItemViewModel
    {
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
    }

    public class AdminOrderDetailsViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderLineViewModel> Lines { get; set; } = new();          // already exists from Section 11
        public List<StatusHistoryItemViewModel> History { get; set; } = new();
        public List<OrderStatus> AllowedNextStatuses { get; set; } = new();   // buttons the admin may press
    }
}