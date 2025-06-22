using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using BookWeb.Utility.ShoppingCartHelper;

namespace BookWeb.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly CartHelper _cartHelper;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository
            , CartHelper cartHelper)
        {
         
            _shoppingCartRepository = shoppingCartRepository;
            _cartHelper = cartHelper;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync()
        {
            var token = _cartHelper.GetCartToken();
            var isAuthenticated = _cartHelper.IsAuthenticated();

            var cart = await _shoppingCartRepository.GetCartAsync(token, isAuthenticated);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = isAuthenticated ? token : null,
                    CartToken = isAuthenticated ? null : token,
                };

                await _shoppingCartRepository.AddCartAsync(cart);
                
            }

            return cart;

        }

        public async Task AddToCartAsync(int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Count += quantity;
            }
            else
            {
                cart.Items.Add(new ShoppingCartItem { ProductId = productId, Count = quantity});
                
            }

            item!.Price = UpdatePriceBasedOnQuantity(item);
            foreach (ShoppingCartItem i in cart.Items)
            {
                cart.CartTotal += i.Price;
            }
            await _shoppingCartRepository.SaveAsync();
        }

        private int UpdatePriceBasedOnQuantity(ShoppingCartItem shoppingCartItem)
        {
            return shoppingCartItem.Count switch
            {
                >= 1 and <= 50 => (int)shoppingCartItem.Product.Price * shoppingCartItem.Count,
                >= 51 and <= 100 => (int)shoppingCartItem.Product.Price50 * shoppingCartItem.Count ,
                > 100 => (int)shoppingCartItem.Product.Price100 * shoppingCartItem.Count,
                _ => throw new ArgumentException("Valore non valido"),
            };
        }
    }
}
