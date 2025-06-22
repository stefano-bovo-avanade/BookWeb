using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IShoppingCartRepository
    {
        public Task<ShoppingCart> GetCartAsync(string token, bool isAuthenticated);

        public Task AddCartAsync(ShoppingCart cart);

        public Task SaveAsync();

    }
}
