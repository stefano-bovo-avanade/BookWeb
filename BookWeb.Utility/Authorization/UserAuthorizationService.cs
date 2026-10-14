using BookWeb.Service.Context;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Utility.Authorization
{
    public class UserAuthorizationService
    {
        private readonly ApplicationDbContext _context;

        public UserAuthorizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(
            string userId,
            string permissionName)
        {
            return await _context.UserRoles
                .AnyAsync(ur =>
                    ur.UserId == userId &&
                    ur.Role.RolePermissions.Any(rp =>
                        rp.Permission.Name == permissionName));
        }
    }
}