using BookWeb.Models.Models;

namespace BookWeb.Service
{
    public interface IShoppingCartService
    {
        public Task<ShoppingCart> GetOrCreateCartAsync();
        public Task AddToCartAsync(int productId, int quantity);
    }
}
