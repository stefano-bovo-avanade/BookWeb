using BookWeb.Models.Models;

namespace BookWeb.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }

        public Order Order { get; set; }
    }
}
