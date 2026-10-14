namespace BookWeb.Models.Models;

public class LoginRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public int? RoleId { get; set; }
    public string? LoginTicket { get; set; }
}