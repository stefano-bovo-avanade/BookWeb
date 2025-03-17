using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IApplicationUserRepository
    {
        public Task<ApplicationUser> GetApplicationUser(string? Id);

        public Task<IEnumerable<ApplicationUser>> GetApplicationUsers();

        //public Task<int> InsertApplicationUser(ApplicationUser ApplicationUser);

        //public Task<int> UpdateApplicationUser(ApplicationUser ApplicationUser);

        //public Task<int> DeleteApplicationUser(int? id);
    }
}
