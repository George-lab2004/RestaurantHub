using RestaurantHub.ViewModel;

namespace RestaurantHub.Services
{
    public interface ICartService
    {
        Task<CartViewModels> BuildAsync();
        Task<bool> AddAsync(int menuItemId, int quantity);
        void SetQuantity(int menuItemId, int quantity);
        void Remove(int menuItemId);
        void Clear();
    }
}