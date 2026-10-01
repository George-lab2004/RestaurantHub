namespace RestaurantHub.Helpers
{
    public static class OrderRules
    {
        public const decimal TaxRate = 0.14m;   // 14%, change as you like

        public static readonly string[] PaymentMethods = { "Cash on delivery", "Card on delivery" };

        public static decimal CalculateTax(decimal subtotal) =>
            Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
    }
}