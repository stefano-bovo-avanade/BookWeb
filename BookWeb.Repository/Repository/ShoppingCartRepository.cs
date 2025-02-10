using BookWeb.Models.Models;
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
                    $"(Id, ProductId, Count, Product, ApplicationUserId, ApplicationUser) " +
                    $"VALUES " +
                    $"(@Id, @ProductId, @Count, @Product, @ApplicationUserId, @ApplicationUser); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                new
                {
                    @ProductId = shoppingCart.ProductId,
                    @Count = shoppingCart.Count,
                    @Product = shoppingCart.Product,
                    @ApplicationUserId = shoppingCart.ApplicationUserId,
                    @ApplicationUser = shoppingCart.ApplicationUser,
                });
                
                return result.FirstOrDefault();
                
            }
        }

        public async Task<int> UpdateShoppingCart(ShoppingCart shoppingCart)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE Categories SET" +
                    $"ProductId  = @ProductId, " +
                    $"Count  = @Count, " +
                    $"Product  = @Product, " +
                    $"ApplicationUserId  = @ApplicationUserId, " +
                    $"ApplicationUser  = @ProApplicationUserductId, " +
                    $"WHERE Id = @Id; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                    new
                    {
                        @ProductId = shoppingCart.ProductId,
                        @Count = shoppingCart.Count,
                        @Product = shoppingCart.Product,
                        @ApplicationUserId = shoppingCart.ApplicationUserId,
                        @ApplicationUser = shoppingCart.ApplicationUser,
                    });
                return result.FirstOrDefault();
            }
        }

        public async Task<int> DeleteShoppingCart(int? id)
        {
            var p = new DynamicParameters();
            p.Add("id", id);
            p.Add("returnValue", dbType:DbType.Int32, direction: ParameterDirection.ReturnValue);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync("[dbo].[DeleteShoppingCart]", p, commandType: CommandType.StoredProcedure);
                var returnValue = p.Get<int>("returnValue");
                return returnValue;
            }
        }
    }
}
