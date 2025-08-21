using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Utility.ShoppingCartHelper;

namespace BookWeb.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly CartHelper _cartHelper;
        private readonly IProductRepository _productRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository
            , CartHelper cartHelper
            , IProductRepository productRepository)
        {
         
            _shoppingCartRepository = shoppingCartRepository;
            _cartHelper = cartHelper;
            _productRepository = productRepository;
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
            else if (isAuthenticated && string.IsNullOrEmpty(cart.UserId))
            {
                // L'utente ha fatto login e il carrello è anonimo → lo convertiamo
                cart.UserId = token;
                cart.CartToken = null;

                await _shoppingCartRepository.UpdateCartAsync(cart);
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
                item.Price = UpdatePriceBasedOnQuantity(item);
            }
            else
            {
                item = new ShoppingCartItem { ProductId = productId, Count = quantity };
                item.Product = await _productRepository.GetProduct(productId);
                item.Price = UpdatePriceBasedOnQuantity(item);
                cart.Items.Add(item);
            }

            UpdateCartTotal(cart);
            await _shoppingCartRepository.SaveAsync();
        }

        private void UpdateCartTotal(ShoppingCart shoppingCart)
        {
            shoppingCart.CartTotal = 0;
            foreach (ShoppingCartItem i in shoppingCart.Items)
            {
                shoppingCart.CartTotal += i.Price;
            }
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

        public async Task Plus(int productId)
        {
            var cartFromDb = await GetOrCreateCartAsync();
            var shoppinCartItem = cartFromDb.Items.Where(s => s.Id == productId).FirstOrDefault();
            shoppinCartItem!.Count += 1;
            shoppinCartItem.Price = UpdatePriceBasedOnQuantity(shoppinCartItem);
            UpdateCartTotal(cartFromDb);
            await SaveChangesAsync();
        }

        public async Task Minus(int productId)
        {
            var cartFromDb = await GetOrCreateCartAsync();
            var shoppinCartItem = cartFromDb.Items.Where(s => s.Id == productId).FirstOrDefault();

            if (shoppinCartItem!.Count == 1)
            {
                await _shoppingCartRepository.RemoveShoppingCartItem(shoppinCartItem);
            }
            else
            {
                shoppinCartItem.Count -= 1;
                shoppinCartItem.Price = UpdatePriceBasedOnQuantity(shoppinCartItem);
                UpdateCartTotal(cartFromDb);
                await SaveChangesAsync();
            }
            
        }

        public async Task Remove(int productId)
        {
            var cartFromDb = await GetOrCreateCartAsync();
            var shoppinCartItem = cartFromDb.Items.Where(s => s.Id == productId).FirstOrDefault();
            await _shoppingCartRepository.RemoveShoppingCartItem(shoppinCartItem);
            UpdateCartTotal(cartFromDb);
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _shoppingCartRepository.SaveAsync();
        }
    }
}
