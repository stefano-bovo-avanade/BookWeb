namespace BookWeb.Models.Models
{
    public class ApplicationUser : BaseUser
    {
        public string? PasswordHash { get; set; }
    }
}
