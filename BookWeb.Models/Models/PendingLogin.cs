namespace BookWeb.Models.Models
{
    public class PendingLogin
    {
        public int Id { get; set; }

        public string Ticket { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string AllowedRoleIds { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}