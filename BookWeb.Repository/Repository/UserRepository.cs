using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;

namespace BookWeb.Repository.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserAsync(string Id)
        {
            return _context.Users.Where(u => u.Id.ToString() == Id).FirstOrDefault();

        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return _context.Users.ToList();
        }
    }
}
