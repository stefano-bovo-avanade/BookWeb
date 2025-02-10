using BookWeb.Service.Context;
using BookWeb.Models.Models;
using Dapper;
using System.Data;

namespace BookWeb.Repository.Repository
{
    public interface IShoppingCartRepository
    {
        public Task<ShoppingCart> GetShoppingCart(int? Id);

        public Task<IEnumerable<ShoppingCart>> GetShoppingCarts();

        public Task<int> InsertShoppingCart(ShoppingCart ShoppingCart);

        public Task<int> UpdateShoppingCart(ShoppingCart ShoppingCart);

        public Task<int> DeleteShoppingCart(int? id);

    }
}
