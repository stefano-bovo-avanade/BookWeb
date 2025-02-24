using BookWeb.Models.Models;
using BookWeb.Service.Context;
using Dapper;

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
            var query = $"SELECT * " +
                $"FROM {Product.F_PRODUCTS} where {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Product>(query, new { Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<VProduct>> GetProducts()
        {
            var query = "SELECT " +
                "[Id], " +
                "[Title], " +
                "[Author], " +
                "[Description], " +
                "[ISBN], " +
                "[ListPrice], " +
                "[Price], " +
                "[Price50], " +
                "[Price100], " +
                "[CategoryId], " +
                "[ImageUrl], " +
                "[Category_Id], " +
                "[Category_Name], " +
                "[Category_DisplayOrder] " +
                "FROM [Bulky].[dbo].[ProductsCategories]";
 
            using (var connection = _context.CreateConnection())
            {
                var products = await connection.QueryAsync<VProduct>(query);
                return products;    
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
                    $"{Product.F_PRICE100}, " +
                    $"{Product.F_CATEGORYID}, " +
                    $"{Product.F_IMAGEURL}) VALUES " +
                    $"(@{Product.F_TITLE}, " +
                    $"@{Product.F_DESCRIPTION}, " +
                    $"@{Product.F_ISBN}, " +
                    $"@{Product.F_AUTHOR}, " +
                    $"@{Product.F_LISTPRICE}, " +
                    $"@{Product.F_PRICE}, " +
                    $"@{Product.F_PRICE50}, " +
                    $"@{Product.F_PRICE100}, " +
                    $"@{Product.F_CATEGORYID}, " +
                    $"@{Product.F_IMAGEURL}); " +
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
                    Price100 = product.Price100,
                    CategoryId = product.CategoryId,
                    ImageUrl = product.ImageUrl,
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
                    $"{Product.F_AUTHOR} = @{Product.F_AUTHOR}, " +
                    $"{Product.F_DESCRIPTION} = @{Product.F_DESCRIPTION}, " +
                    $"{Product.F_ISBN} = @{Product.F_ISBN}, " +
                    $"{Product.F_LISTPRICE} = @{Product.F_LISTPRICE}, " +
                    $"{Product.F_PRICE} = @{Product.F_PRICE}, " +
                    $"{Product.F_PRICE50} = @{Product.F_PRICE50}, " +
                    $"{Product.F_PRICE100} = @{Product.F_PRICE100}, " +
                    $"{Product.F_CATEGORYID} = @{Product.F_CATEGORYID}, " +
                    $"{Product.F_IMAGEURL} = @{Product.F_IMAGEURL} " +
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
                    Price100 = product.Price100,
                    CategoryId = product.CategoryId,
                    ImageUrl = product.ImageUrl,
                });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteProduct(int? id)
        {
            string sql = $"DELETE from {Product.F_PRODUCTS} where {Product.F_ID} = @{Product.F_ID}";
            
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { id = id});
                return result;
            }
            
        }
    }
}
