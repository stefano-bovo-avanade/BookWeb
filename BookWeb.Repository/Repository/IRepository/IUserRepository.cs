using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IUserRepository
    {
        public Task<User> GetUserAsync(string? Id);

        public Task<IEnumerable<User>> GetUsers();

        //public Task<int> InsertUser(User User);

        //public Task<int> UpdateUser(User User);

        //public Task<int> DeleteUser(int? id);
    }
}
