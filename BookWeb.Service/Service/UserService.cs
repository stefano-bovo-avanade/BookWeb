using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;

namespace BookWeb.Service.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserAsync(string userId) 
        {
            return await _userRepository.GetUserAsync(userId);
        }

    }
}
