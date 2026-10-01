using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RestaurantHub.ViewModel
{
    public class MenuItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name can't exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters")]
        public string? Description { get; set; }

        [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "Price must be between 0.01 and 100000")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}