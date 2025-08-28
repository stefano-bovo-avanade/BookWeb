using BookWeb.Models.Models;

namespace BookWeb.Service.Service.IService
{
    public interface IShoppingCartService
    {
        public Task<ShoppingCart> GetOrCreateCartAsync();
        public Task MergeGuestCartAsync(string guestCartToken, string userId);
        public Task OrderConfirmation(int orderId, int cartId);
        public Task AddToCartAsync(int productId, int quantity);
        public Task Plus(int productId);
        public Task Minus(int productId);
        public Task Remove(int productId);
        public Task SaveChangesAsync();
    }
}
