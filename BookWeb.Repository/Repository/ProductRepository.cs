using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
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

        // Recupera un prodotto tramite ID
        public async Task<Product?> GetProduct(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return null;
            }

            var query = $"SELECT * " +
                        $"FROM {Product.F_PRODUCTS} " +
                        $"WHERE {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var product = await connection.QuerySingleOrDefaultAsync<Product>(
                    query,
                    new
                    {
                        Id = id.Value
                    });

                return product;
            }
        }

        // Recupera tutti i prodotti con i dati delle categorie
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

        // Inserisce un nuovo prodotto
        public async Task<int> InsertProduct(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var query = $"INSERT INTO {Product.F_PRODUCTS} " +
                        $"({Product.F_TITLE}, " +
                        $"{Product.F_DESCRIPTION}, " +
                        $"{Product.F_ISBN}, " +
                        $"{Product.F_AUTHOR}, " +
                        $"{Product.F_LISTPRICE}, " +
                        $"{Product.F_PRICE}, " +
                        $"{Product.F_PRICE50}, " +
                        $"{Product.F_PRICE100}, " +
                        $"{Product.F_CATEGORYID}, " +
                        $"{Product.F_IMAGEURL}) " +
                        "VALUES " +
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
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = _context.CreateConnection())
            {
                var id = await connection.QuerySingleAsync<int>(
                    query,
                    new
                    {
                        Title = product.Title,
                        Description = product.Description,
                        ISBN = product.ISBN,
                        Author = product.Author,
                        ListPrice = product.ListPrice,
                        Price = product.Price,
                        Price50 = product.Price50,
                        Price100 = product.Price100,
                        CategoryId = product.CategoryId,
                        ImageUrl = product.ImageUrl
                    });

                return id;
            }
        }

        // Aggiorna un prodotto esistente
        public async Task<int> UpdateProduct(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (product.Id <= 0)
            {
                return 0;
            }

            var query = $"UPDATE {Product.F_PRODUCTS} SET " +
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
                        $"WHERE {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(
                    query,
                    new
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Author = product.Author,
                        Description = product.Description,
                        ISBN = product.ISBN,
                        ListPrice = product.ListPrice,
                        Price = product.Price,
                        Price50 = product.Price50,
                        Price100 = product.Price100,
                        CategoryId = product.CategoryId,
                        ImageUrl = product.ImageUrl
                    });

                return rowsAffected;
            }
        }

        // Elimina un prodotto tramite ID
        public async Task<int> DeleteProduct(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return 0;
            }

            var query = $"DELETE FROM {Product.F_PRODUCTS} " +
                        $"WHERE {Product.F_ID} = @{Product.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(
                    query,
                    new
                    {
                        Id = id.Value
                    });

                return rowsAffected;
            }
        }
    }
}