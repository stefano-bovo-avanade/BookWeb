using BookWeb.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.ViewModels
{
    public class CheckoutVM
    {
        public IEnumerable<ShoppingCartItem> Items { get; set; }

        public int OrderTotal { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string? StreetAddress { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public string? PostalCode { get; set; }
    }
}
