using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RestaurantHub.Models;

namespace RestaurantHub.ViewModel
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Delivery address is required")]
        [StringLength(300, ErrorMessage = "Address can't exceed 300 characters")]
        [Display(Name = "Delivery address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notes can't exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Please choose a payment method")]
        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [BindNever]   // display only: never accept this from the form
        public CartViewModels? Cart { get; set; }
    }

    public class OrderLineViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class OrderDetailsViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }

        public List<StatusHistoryItemViewModel> History { get; set; } = new();   
        public bool CanCancel { get; set; }
        public List<OrderLineViewModel> Lines { get; set; } = new();
    }
}