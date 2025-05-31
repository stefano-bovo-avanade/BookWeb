using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Utility;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : BaseController
    {
       //[BindProperty]
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IMapper _mapper;

        public CartController(IShoppingCartRepository shoppingCartRepository, 
            IUserRepository userRepository,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IMapper mapper)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _userRepository = userRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result,
                OrderHeader = new(),
            };
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }
            return View(shoppingCartVM);
            
        }

        public IActionResult Summary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result,
                OrderHeader = new(),
            };

            shoppingCartVM.OrderHeader.User = _userRepository.GetUser(userId).Result;
            shoppingCartVM.OrderHeader = _mapper.From(shoppingCartVM.OrderHeader.User).AdaptTo(shoppingCartVM.OrderHeader);

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            shoppingCartVM.ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result;
            shoppingCartVM.OrderHeader.User = _userRepository.GetUser(userId).Result;
            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.ShippingDate = System.DateTime.Now.AddDays(7);
            shoppingCartVM.OrderHeader.UserId = userId;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }

            if (shoppingCartVM.OrderHeader.User.CompanyId.GetValueOrDefault() == 0)
            {
                // è un cliente regolare e dobbiamo verificare il pagamento
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // utente company
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;

            }

            var orderInsertedId = _orderHeaderRepository.InsertOrderHeader(shoppingCartVM.OrderHeader).Result;
            var OrderHeaderInserted = _orderHeaderRepository.GetOrderHeader(orderInsertedId).Result;
            OrderHeaderInserted.User = _userRepository.GetUser(OrderHeaderInserted.UserId).Result;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = OrderHeaderInserted.Id,
                    Count = cart.Count,
                    Price = cart.Price,
                };
                var r = _orderDetailRepository.InsertOrderDetail(orderDetail);
            }

            #region

            if (OrderHeaderInserted.User.CompanyId.GetValueOrDefault() == 0)
            {
                // è un cliente regolare e dobbiamo verificare il pagamento
                StripeConfiguration.ApiKey = "sk_test_51R7aotGdXWKcYFAs2sZ5ESMHEs5J4HBCI7WtgSbhpIE3MoCMlOg6oXS0h0kSjRmehAF8e3afnsbKEp5y3kbIWGfi00i9yZmmBx";

                var domain = "https://localhost:7046/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={orderInsertedId}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,

                            }
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);
                _orderHeaderRepository.UpdateStripePaymentID(orderInsertedId, session.Id, session.PaymentIntentId);
                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);

            }
            #endregion

            return RedirectToAction(nameof(OrderConfirmation), new { id = OrderHeaderInserted.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _orderHeaderRepository.GetOrderHeader(id).Result;

            #region

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _orderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                }
            }

            #endregion

            List<ShoppingCart> shoppingCarts = _shoppingCartRepository.GetShoppingCartsPerUserId(orderHeader.UserId).Result.ToList();
            foreach (var cart in shoppingCarts)
            {
                _shoppingCartRepository.DeleteShoppingCart(cart.Id);
            }

            return View(id);
        }

        public IActionResult Plus(int cartId) 
        {
            var cartFromDb = _shoppingCartRepository.GetShoppingCart(cartId).Result;
            cartFromDb.Count += 1;
            _shoppingCartRepository.UpdateShoppingCart(cartFromDb);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _shoppingCartRepository.GetShoppingCart(cartId).Result;
            if (cartFromDb.Count == 1)
            {
                _shoppingCartRepository.DeleteShoppingCart(cartId);
            }
            else
            {
                cartFromDb.Count -= 1;
                _shoppingCartRepository.UpdateShoppingCart(cartFromDb);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            _shoppingCartRepository.DeleteShoppingCart(cartId);
            return RedirectToAction(nameof(Index));
        }

        private int GetPriceBasedOnQuantity(ShoppingCart cart) 
        {
            return cart.Count switch
            {
                >= 1 and <= 50 => (int)cart.Product.Price * cart.Count,
                >= 51 and <= 100 => (int)cart.Product.Price50 * cart.Count,
                > 100 => (int)cart.Product.Price100 * cart.Count,
                _ => throw new ArgumentException("Valore non valido"),
            };
        }
    }
}
