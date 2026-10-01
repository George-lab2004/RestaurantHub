using Microsoft.AspNetCore.Identity;

namespace RestaurantHub.Models
{
    public class ApplicationUser : IdentityUser
    {
  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Address { get; set; }
        public List<Order> Orders { get; set; }
    }
}
