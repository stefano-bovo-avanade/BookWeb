using BookWeb.Models.Models;
using BookWeb.Service.Context;
using Dapper;
using System.Data;

namespace BookWeb.Repository.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly DapperContext _context;
        public ProductRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProduct(int? Id)
        {
            var query = $"SELECT " +
                $"{Product.F_TITLE}, " +
                $"{Product.F_DESCRIPTION}, " +
                $"{Product.F_ISBN}," +
                $"{Product.F_AUTHOR}," +
                $"{Product.F_LISTPRICE}," +
                $"{Product.F_PRICE}," +
                $"{Product.F_PRICE50}," +
                $"{Product.F_PRICE100} " +
                $"FROM {Product.F_PRODUCTS} where {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Product>(query, new { @Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var query = $"SELECT * FROM {Product.F_PRODUCTS}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Product>(query);
                return result.ToList();
            }
        }

        public async Task<int> InsertProduct(Product product)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"INSERT INTO {Product.F_PRODUCTS} " +
                    $"({Product.F_TITLE}, " +
                    $"{Product.F_DESCRIPTION}, " +
                    $"{Product.F_ISBN}, " +
                    $"{Product.F_AUTHOR}, " +
                    $"{Product.F_LISTPRICE}, " +
                    $"{Product.F_PRICE}, " +
                    $"{Product.F_PRICE50}, " +
                    $"{Product.F_PRICE100}) VALUES " +
                    $"(@{Product.F_TITLE}, " +
                    $"@{Product.F_DESCRIPTION}, " +
                    $"@{Product.F_ISBN}, " +
                    $"@{Product.F_AUTHOR}, " +
                    $"@{Product.F_LISTPRICE}, " +
                    $"@{Product.F_PRICE}, " +
                    $"@{Product.F_PRICE50}, " +
                    $"@{Product.F_PRICE100}); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql, new 
                { 
                    Title = product.Title,
                    Description = product.Description,
                    Isbn = product.ISBN,
                    Author = product.Author,
                    ListPrice = product.ListPrice,
                    Price = product.Price,
                    Price50 = product.Price50,
                    Price100 = product.Price100
                });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> UpdateProduct(Product product)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE {Product.F_PRODUCTS} SET " +
                    $"{Product.F_TITLE} = @{Product.F_TITLE}, " +
                    $"{Product.F_DESCRIPTION} = @{Product.F_DESCRIPTION}, " +
                    $"{Product.F_ISBN} = @{Product.F_ISBN}, " +
                    $"{Product.F_AUTHOR} = @{Product.F_AUTHOR}," +
                    $"{Product.F_LISTPRICE} = @{Product.F_LISTPRICE}, " +
                    $"{Product.F_PRICE} = @{Product.F_PRICE}, " +
                    $"{Product.F_PRICE50} = @{Product.F_PRICE50}, " +
                    $"{Product.F_PRICE100} = @{Product.F_PRICE100} " +
                    $"WHERE {Product.F_ID} = @{Product.F_ID}; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql, new
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Isbn = product.ISBN,
                    Author = product.Author,
                    ListPrice = product.ListPrice,
                    Price = product.Price,
                    Price50 = product.Price50,
                    Price100 = product.Price100
                });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteProduct(int? id)
        {
            var p = new DynamicParameters();
            p.Add("id", id);
            p.Add("returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync("[dbo].[DeleteCategory]", p, commandType: CommandType.StoredProcedure);
                var returnValue = p.Get<int>("returnValue");
                return returnValue;
            }
        }
    }
}
