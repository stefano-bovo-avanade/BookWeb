using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IProductRepository
    {
        public Task<Product> GetProduct(int? Id);

        public Task<IEnumerable<VProduct>> GetProducts();

        public Task<int> InsertProduct(Product Product);

        public Task<int> UpdateProduct(Product Product);

        public Task<int> DeleteProduct(int? id);
    }
}
