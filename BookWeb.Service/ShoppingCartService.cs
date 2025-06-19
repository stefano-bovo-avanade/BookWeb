using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using BookWeb.Utility.ShoppingCartHelper;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly CartHelper _cartHelper;

        public ShoppingCartService(ApplicationDbContext context, IShoppingCartRepository shoppingCartRepository
            , CartHelper cartHelper)
        {
            _context = context;
            _shoppingCartRepository = shoppingCartRepository;
            _cartHelper = cartHelper;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync()
        {
            var token = _cartHelper.GetCartToken();
            var isAuthenticated = _cartHelper.IsAuthenticated();

            var cart = await _context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c =>
            isAuthenticated ? c.UserId == token : c.CartToken == token);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = isAuthenticated ? token : null,
                    CartToken = isAuthenticated ? null : token,
                };

                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
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

            item.Price = UpdatePriceBasedOnQuantity(item);
            foreach (ShoppingCartItem i in cart.Items)
            {
                cart.cartTotal += i.Price;
            }
            await _context.SaveChangesAsync();
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
