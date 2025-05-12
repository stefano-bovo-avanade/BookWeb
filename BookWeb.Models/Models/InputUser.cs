using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class InputUser : BaseUser
    {
        [Required]
        public string? Password { get; set; }
    }
}
