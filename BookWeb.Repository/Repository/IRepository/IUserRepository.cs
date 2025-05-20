using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IUserRepository
    {
        public Task<User> GetUser(string? Id);

        public Task<IEnumerable<User>> GetUsers();

        //public Task<int> InsertApplicationUser(ApplicationUser ApplicationUser);

        //public Task<int> UpdateApplicationUser(ApplicationUser ApplicationUser);

        //public Task<int> DeleteApplicationUser(int? id);
    }
}
