using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using BookWeb.Utility.ShoppingCartHelper;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Order = BookWeb.Models.Models.Order;

namespace BookWeb.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly CartHelper _cartHelper;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IConfiguration _configuration;

        public OrderService(IShoppingCartRepository shoppingCartRepository,
            CartHelper cartHelper,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IConfiguration configuration)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _cartHelper = cartHelper;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _configuration = configuration;
        }


        public async Task<Order> GetOrderAsync(int userId) 
        {
            return await _orderRepository.GetOrderAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<IEnumerable<Order>> GetOrderPerUserIdAsync(string UserId)
        {
            return await _orderRepository.GetOrderPerUserIdAsync(UserId);
        }

        public async Task<int> CreateOrderAsync(string userId, Order order)
        {
            var cart = await _shoppingCartRepository.GetCartByUserIdAsync(userId);
            var user = await _userRepository.GetUserAsync(userId);

            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Il carrello è vuoto.");

            order.User = user;
            // Logica per differenziare il pagamento tra privati e aziende
            SetOrderStatusBasedOnUser(order, user);

            var orderDetails = cart.Items.Select(shoppingItem => new OrderDetail
            {
                ProductId = shoppingItem.ProductId,
                Count = shoppingItem.Count,
                Price = shoppingItem.Price,
                Product = shoppingItem.Product,
            }).ToList();

            order.OrderTotal = orderDetails.Sum(od => od.Price);
            var orderId = await _orderRepository.InsertOrder(order);
            foreach (var detail in orderDetails)
            {
                detail.OrderId = orderId;
                await _orderDetailRepository.InsertOrderDetail(detail);
            }

            return orderId;
        }

        public async Task<int> UpdateOrder(Order Order) 
        {
            return await _orderRepository.UpdateOrder(Order);
        }


        public async Task<string?> CreateStripeSessionAsync(int orderId)
        {
            var cart = await _cartHelper.GetCurrentCartAsync();

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return null;

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var domain = _configuration["Stripe:Domain"];

            var options = new SessionCreateOptions
            {
                SuccessUrl = $"https://localhost:7046/customer/cart/OrderConfirmation?orderId={orderId}&cartId={cart.Id}",
                CancelUrl = $"https://localhost:7046/customer/cart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            foreach (var item in cart.Items)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        
                        UnitAmount = item.UnitPrice * 100,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            var session = service.Create(options);

            await UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);

            return session.Url;
        }

        public async Task UpdateOrderStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            Order header = await GetOrderAsync(id);

            if (header != null)
            {
                header.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    header.PaymentStatus = paymentStatus;
                }
                var result = _orderRepository.UpdateOrder(header);
            }
        }

        public async Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            Order header = await GetOrderAsync(id);

            if (header != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    header.SessionId = sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    header.PaymentIntentId = paymentIntentId;
                    header.PaymentDate = DateTime.Now;
                }
                var result = _orderRepository.UpdateOrder(header);
            }
        }

        private void SetOrderStatusBasedOnUser(Order order, User user)
        {
            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                order.PaymentStatus = SD.PaymentStatusPending;
                order.OrderStatus = SD.StatusPending;
            }
            else
            {
                order.PaymentStatus = SD.PaymentStatusDelayedPayment;
                order.OrderStatus = SD.StatusApproved;
            }
        }

    }
}
