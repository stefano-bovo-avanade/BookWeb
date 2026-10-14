using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string? StreetAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company Company { get; set; }

        public Role Role { get; set; }

        public int RoleId { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
