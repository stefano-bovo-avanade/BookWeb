namespace BookWeb.Models.Models
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Vecchia relazione, la manteniamo per non rompere il codice esistente
        public ICollection<User> Users { get; set; }
            = new List<User>();

        // Nuova relazione User <-> Role
        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();

        // Nuova relazione Role <-> Permission
        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}