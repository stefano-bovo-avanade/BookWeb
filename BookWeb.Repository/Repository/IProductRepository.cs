using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository
{
    public interface IProductRepository
    {
        public Task<Product> GetProduct(int? Id);

        public Task<IEnumerable<Product>> GetProducts();

        public Task<int> InsertProduct(Product Product);

        public Task<int> UpdateProduct(Product Product);

        public Task<int> DeleteProduct(int? id);
    }
}
