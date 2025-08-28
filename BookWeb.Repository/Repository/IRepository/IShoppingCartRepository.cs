using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IShoppingCartRepository
    {
        public Task<ShoppingCart?> GetCartByIdAsync(int id);
        public Task<ShoppingCart?> GetCartByUserIdAsync(string userId);
        public Task<ShoppingCart?> GetCartByCartTokenAsync(string cartToken);
        public Task AddCartAsync(ShoppingCart cart);
        public Task UpdateCartAsync(ShoppingCart cart);
        public Task RemoveShoppingCart(ShoppingCart shoppingCart);
        public Task RemoveShoppingCartItem(ShoppingCartItem shoppingCartItem);
        public Task SaveAsync();

    }
}
