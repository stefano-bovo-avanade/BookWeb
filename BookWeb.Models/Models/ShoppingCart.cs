namespace BookWeb.Models.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // null per utenti anonimi
        public string? CartToken { get; set; } // usato solo per utenti non loggati
        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public int cartTotal { get; set; }
    }
}
