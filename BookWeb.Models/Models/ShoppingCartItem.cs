using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public int ShoppingCartId { get; set; }

        public string? CartToken { get; set; } // usato solo per utenti non loggati

        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        public int Price { get; set; }
    }
}
