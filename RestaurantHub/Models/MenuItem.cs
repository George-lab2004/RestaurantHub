using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RestaurantHub.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        [MaxLength(300)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}