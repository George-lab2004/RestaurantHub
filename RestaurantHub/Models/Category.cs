using System.ComponentModel.DataAnnotations;

namespace RestaurantHub.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ImageUrl { get; set; }

        public List<MenuItem> MenuItems { get; set; } = new();
    }
}