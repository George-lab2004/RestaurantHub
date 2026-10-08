using RestaurantHub.Models;

namespace RestaurantHub.Helpers
{
    public static class OrderRules
    {
        public const decimal TaxRate = 0.14m;   // 14%, change as you like

        public static readonly string[] PaymentMethods = { "Cash on delivery", "Card on delivery" };

        public static decimal CalculateTax(decimal subtotal) =>
            Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);

        // Allowed status transitions for an order
        private static readonly Dictionary<OrderStatus, OrderStatus[]> StatusTransitions =
            new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending,   new[] { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new[] { OrderStatus.Preparing, OrderStatus.Cancelled } },
                { OrderStatus.Preparing, new[] { OrderStatus.Ready } },
                { OrderStatus.Ready,     new[] { OrderStatus.Delivered } },
                { OrderStatus.Delivered, Array.Empty<OrderStatus>() },
                { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
            };

        public static IReadOnlyList<OrderStatus> GetAllowedNextStatuses(OrderStatus current) =>
            StatusTransitions.TryGetValue(current, out var next) ? next : Array.Empty<OrderStatus>();
        // The customer may cancel only while the order is still Pending (nobody accepted it yet)
        public static bool CanCustomerCancel(OrderStatus current) => current == OrderStatus.Pending;
        public static bool CanTransition(OrderStatus current, OrderStatus next) =>
            GetAllowedNextStatuses(current).Contains(next);
    }
}