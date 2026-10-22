using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IProductRepository
    {
        Task<Product?> GetProduct(int? id);

        Task<IEnumerable<VProduct>> GetProducts();

        Task<int> InsertProduct(Product product);

        Task<int> UpdateProduct(Product product);

        Task<int> DeleteProduct(int? id);
    }
}