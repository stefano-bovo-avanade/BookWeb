using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;
using System.Data;

namespace BookWeb.Repository.Repository
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly DapperContext _context;
        public ShoppingCartRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart> GetShoppingCart(int? Id)
        {
            var query = $"SELECT * FROM ShoppingCarts where Id = @Id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<ShoppingCart>(query, new { @Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<ShoppingCart>> GetShoppingCartsPerUserId(string? UserId)
        {
            var query = $"SELECT * FROM ShoppingCarts where UserId = @UserId;" +
                        $"SELECT * FROM Products;";

            using (var connection = _context.CreateConnection())
            {
                List<ShoppingCart>? shoppingCarts = null;
                List<Product>? products = null;
                using (var lists = await connection.QueryMultipleAsync(query, new { UserId = UserId}))
                {
                    shoppingCarts = lists.Read<ShoppingCart>().ToList();
                    products = lists.Read<Product>().ToList();
                }
                
                foreach (var shoppingCart in shoppingCarts)
                {
                    shoppingCart.Product = products.Where(p => p.Id == shoppingCart.ProductId).FirstOrDefault();
                }
                return shoppingCarts;
                
            }
        }

        public async Task<IEnumerable<ShoppingCart>> GetShoppingCarts()
        {
            var query = $"SELECT * FROM ShoppingCarts";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<ShoppingCart>(query);
                return result.ToList();
            }
        }

        public async Task<int> InsertShoppingCart(ShoppingCart shoppingCart)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"INSERT INTO ShoppingCarts " +
                    $"(ProductId, Count, UserId) " +
                    $"VALUES " +
                    $"(@ProductId, @Count, @UserId); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                new
                {
                    ProductId = shoppingCart.ProductId,
                    Count = shoppingCart.Count,
                    UserId = shoppingCart.UserId,
                });
                
                return result.FirstOrDefault();
                
            }
        }

        public async Task<int> UpdateShoppingCart(ShoppingCart shoppingCart)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE ShoppingCarts SET " +
                    $"ProductId  = @ProductId, " +
                    $"[Count] = @Count, " +
                    $"UserId  = @UserId " +
                    $"WHERE Id = @Id; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                    new
                    {
                        ProductId = shoppingCart.ProductId,
                        Count = shoppingCart.Count,
                        UserId = shoppingCart.UserId,
                        Id = shoppingCart.Id
                    });
                return result.FirstOrDefault();
            }
        }

        public async Task<int> DeleteShoppingCart(int? id)
        {
            string query = "Delete from dbo.ShoppingCarts where id = @id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new { id = id });
                return result;
            }
        }
    }
}
