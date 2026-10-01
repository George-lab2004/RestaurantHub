namespace RestaurantHub.ViewModel
{
    public class CategoryListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int ItemsCount { get; set; }
    }
}