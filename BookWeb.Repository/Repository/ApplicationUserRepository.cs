using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;

namespace BookWeb.Repository.Repository
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetApplicationUser(string Id)
        {
            return _context.Users.Where(a => a.Id.ToString() == Id).FirstOrDefault();

        }

        public async Task<IEnumerable<ApplicationUser>> GetApplicationUsers()
        {
            return _context.Users.ToList();
        }
    }
}
