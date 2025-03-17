using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IShoppingCartRepository
    {
        public Task<ShoppingCart> GetShoppingCart(int? Id);

        public Task<IEnumerable<ShoppingCart>> GetShoppingCartsPerUserId(string? UserId);

        public Task<IEnumerable<ShoppingCart>> GetShoppingCarts();

        public Task<int> InsertShoppingCart(ShoppingCart ShoppingCart);

        public Task<int> UpdateShoppingCart(ShoppingCart ShoppingCart);

        public Task<int> DeleteShoppingCart(int? id);

    }
}
