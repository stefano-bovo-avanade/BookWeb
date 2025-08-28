using BookWeb.Models.Models;

namespace BookWeb.Service.Service.IService
{
    public interface IUserService
    {
        public Task<User> GetUserAsync(string userId);
        
    }
}
