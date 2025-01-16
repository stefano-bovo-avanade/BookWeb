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
            var query = $"SELECT * " +
                $"FROM {Product.F_PRODUCTS} where {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Product>(query, new { Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var query = $"SELECT * FROM {Product.F_PRODUCTS};" +
                        $"SELECT * FROM {Category.F_CATEGORY};";
            
            using (var connection = _context.CreateConnection())
            {
                List<Product> products = null;
                List<Category> categories = null;
                using (var lists = await connection.QueryMultipleAsync(query))
                {
                    products = lists.Read<Product>().ToList();
                    categories = lists.Read<Category>().ToList();
                }

                foreach (var product in products)
                {
                    product.Category = categories.Where(c => c.Id == product.CategoryId).Single();
                }
                return products;    
            }
        }

        //public async Task<IEnumerable<Product>> GetProducts()
        //{
        //    var query = "SELECT " +
        //        $"P.{Product.F_ID} as Id, " +
        //        $"P.{Product.F_TITLE} as Title, " +
        //        $"P.{Product.F_AUTHOR} as Author, " +
        //        $"P.{Product.F_DESCRIPTION} as [Description], " +
        //        $"P.{Product.F_ISBN} as ISBN, " +
        //        $"P.{Product.F_LISTPRICE} as ListPrice, " +
        //        $"P.{Product.F_PRICE} as Price, " +
        //        $"P.{Product.F_PRICE50} as Price50, " +
        //        $"P.{Product.F_PRICE100} as Price100, " +
        //        $"C.{Category.F_ID} as [CategoryId], " +
        //        $"C.{Category.F_NOME} as [CategoryName] " +
        //        $"from {Product.F_PRODUCTS} as P " +
        //        $"inner join {Category.F_CATEGORY} as C on P.{Product.F_CATEGORYID} = C.{Category.F_ID}";

        //    using (var connection = _context.CreateConnection())
        //    {
        //        var result = await connection.QueryAsync<Product, Category, Product>(query,
        //            (product, category) =>
        //            {
        //                product.Category = category;
        //                return product;
        //            },
        //            splitOn: "CategoryId");

        //        return result.ToList();
        //    }
        //}

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
