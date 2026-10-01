using System.ComponentModel.DataAnnotations;

namespace RestaurantHub.ViewModel
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "EmailAddress is used")]
        public string Email { get; set; }
    }
}
