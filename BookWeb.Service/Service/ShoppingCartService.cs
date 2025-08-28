using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using BookWeb.Utility.ShoppingCartHelper;
using Stripe.Checkout;

namespace BookWeb.Service.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly CartHelper _cartHelper;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository,
            CartHelper cartHelper,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IOrderService orderService)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _cartHelper = cartHelper;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync()
        {
            var token = _cartHelper.GetCartToken();
            var isAuthenticated = _cartHelper.IsAuthenticated();
            var cart = await _cartHelper.GetCurrentCartAsync();

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
                item.UnitPrice = SetUnitPriceBasedOnQuantity(item);
                item.Price = UpdatePriceBasedOnQuantity(item);
            }
            else
            {
                item = new ShoppingCartItem { ProductId = productId, Count = quantity };
                item.Product = await _productRepository.GetProduct(productId);
                item.UnitPrice = SetUnitPriceBasedOnQuantity(item);
                item.Price = UpdatePriceBasedOnQuantity(item);
                cart.Items.Add(item);
            }

            UpdateCartTotal(cart);
            await _shoppingCartRepository.SaveAsync();
        }

        public async Task MergeGuestCartAsync(string guestCartToken, string userId)
        {
            if (string.IsNullOrEmpty(guestCartToken) || string.IsNullOrEmpty(userId))
                return;

            // Carrello guest (identificato da CartToken)
            var guestCart = await _shoppingCartRepository.GetCartByCartTokenAsync(guestCartToken);
            if (guestCart == null)
                return;

            // Carrello utente (identificato da UserId)
            var userCart = await _shoppingCartRepository.GetCartByUserIdAsync(userId);

            if (userCart == null)
            {
                // Se l'utente non ha un carrello, assegniamo quello del guest
                guestCart.UserId = userId;
                guestCart.CartToken = null;

                await _shoppingCartRepository.SaveAsync();
                return;
            }

            // Altrimenti fai il merge degli articoli
            foreach (var guestItem in guestCart.Items)
            {
                var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Count += guestItem.Count;
                }
                else
                {
                    userCart.Items.Add(new ShoppingCartItem
                    {
                        ProductId = guestItem.ProductId,
                        Count = guestItem.Count,
                        Product = guestItem.Product,
                        Price = guestItem.Price,
                    });
                }    
            }
            UpdateCartTotal(userCart);

            // Elimina il carrello guest
            await _shoppingCartRepository.RemoveShoppingCart(guestCart);
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

        public async Task OrderConfirmation(int orderId, int cartId)
        {
            // 1️) Recupera l'ordine dal DB
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order == null)
                throw new Exception($"Ordine con ID {orderId} non trovato.");

            #region Controllo pagamento Stripe
            if (order.PaymentStatus != SD.PaymentStatusDelayedPayment && !string.IsNullOrEmpty(order.SessionId))
            {
                var service = new SessionService();
                Session session = service.Get(order.SessionId);

                if (session.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    await _orderService.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
                    await _orderService.UpdateOrderStatus(orderId, SD.StatusApproved, SD.PaymentStatusApproved);
                }
            }
            #endregion

            // 2️) Recupera il carrello usando cartId
            var cart = await _shoppingCartRepository.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                // 3️) Elimina tutti gli item del carrello senza triggerare la creazione automatica
                if (cart.Items != null && cart.Items.Any())
                {
                    foreach (var cartItem in cart.Items.ToList()) // ToList per evitare problemi di enumerazione
                    {
                        await _shoppingCartRepository.RemoveShoppingCartItem(cartItem);
                    }
                }

                // 4️) Rimuove il carrello stesso
                await _shoppingCartRepository.RemoveShoppingCart(cart);
            }

        }

        
        private int SetUnitPriceBasedOnQuantity(ShoppingCartItem shoppingCartItem)
        {
            return shoppingCartItem.Count switch
            {
                >= 1 and <= 50 => (int)shoppingCartItem.Product.Price,
                >= 51 and <= 100 => (int)shoppingCartItem.Product.Price50,
                > 100 => (int)shoppingCartItem.Product.Price100,
                _ => throw new ArgumentException("Valore non valido"),
            };
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
            shoppinCartItem.UnitPrice = SetUnitPriceBasedOnQuantity(shoppinCartItem);
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
                UpdateCartTotal(cartFromDb);
                await SaveChangesAsync();
            }
            else
            {
                shoppinCartItem.Count -= 1;
                shoppinCartItem.UnitPrice = SetUnitPriceBasedOnQuantity(shoppinCartItem);
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
