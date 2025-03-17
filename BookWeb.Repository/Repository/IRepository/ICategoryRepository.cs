using BookWeb.Models;
using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface ICategoryRepository
    {
        public Task<Category> GetCategory(int? Id);

        public Task<IEnumerable<Category>> GetCategories();

        public Task<int> InsertCategory(Category category);

        public Task<int> UpdateCategory(Category category);

        public Task<int> DeleteCategory(int? id);
    }
}
