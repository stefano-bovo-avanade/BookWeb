using System.Linq.Expressions;

namespace BookWeb.Repository.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(int Id);

        Task<int> Add(T entity);

        Task<int> Update(T entity);

        Task<int> Remove(T entity);

        //Task<int> RemoveRange(IEnumerable<T> entity);
    }
}
