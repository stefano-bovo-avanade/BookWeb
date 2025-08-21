using BookWeb.Models.Models;

namespace BookWeb.Service
{
    public interface IShoppingCartService
    {
        public Task<ShoppingCart> GetOrCreateCartAsync();
        public Task AddToCartAsync(int productId, int quantity);
        public Task Plus(int productId);
        public Task Minus(int productId);
        public Task Remove(int productId);
        public Task SaveChangesAsync();
    }
}
