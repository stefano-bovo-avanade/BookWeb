using BookWeb.Models.ViewModels;
using BookWeb.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : BaseController
    {
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IShoppingCartService _shoppingCartService;
        
        public CartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _shoppingCartService.GetOrCreateCartAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int Id, int quantity)
        {
            await _shoppingCartService.AddToCartAsync(Id, quantity);

            return RedirectToAction(
             actionName: "ProductDetail",
             controllerName: "Home",
             routeValues: new { area = "Customer", productId = Id }
            );

        }


        public async Task<IActionResult> Summary()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Reindirizza al login con returnUrl per tornare al summary dopo l'autenticazione
                return RedirectToAction(
                actionName: "Login",
                controllerName: "Account",
                routeValues: new { area = "Customer", returnUrl = Url.Action("Summary", "Cart") });

            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Recupera o crea il carrello per l'utente loggato
            var cart = await _shoppingCartService.GetOrCreateCartAsync();

            return View(cart);
        }


        //[HttpPost]
        //[ActionName("Summary")]
        //public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    shoppingCartVM.ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result;
        //    shoppingCartVM.Order.User = _userRepository.GetUser(userId).Result;
        //    shoppingCartVM.Order.OrderDate = System.DateTime.Now;
        //    shoppingCartVM.Order.ShippingDate = System.DateTime.Now.AddDays(7);
        //    shoppingCartVM.Order.UserId = userId;

        //    foreach (var cart in shoppingCartVM.ShoppingCartList)
        //    {
        //        cart.Price = GetPriceBasedOnQuantity(cart);
        //        shoppingCartVM.Order.OrderTotal += cart.Price;
        //    }

        //    if (shoppingCartVM.Order.User.CompanyId.GetValueOrDefault() == 0)
        //    {
        //        // è un cliente regolare e dobbiamo verificare il pagamento
        //        shoppingCartVM.Order.PaymentStatus = SD.PaymentStatusPending;
        //        shoppingCartVM.Order.OrderStatus = SD.StatusPending;
        //    }
        //    else
        //    {
        //        // utente company
        //        shoppingCartVM.Order.PaymentStatus = SD.PaymentStatusDelayedPayment;
        //        shoppingCartVM.Order.OrderStatus = SD.StatusApproved;

        //    }

        //    var orderInsertedId = _orderRepository.InsertOrder(shoppingCartVM.Order).Result;
        //    var OrderInserted = _orderRepository.GetOrder(orderInsertedId).Result;
        //    OrderInserted.User = _userRepository.GetUser(OrderInserted.UserId).Result;

        //    foreach (var cart in shoppingCartVM.ShoppingCartList)
        //    {
        //        OrderDetail orderDetail = new OrderDetail()
        //        {
        //            ProductId = cart.ProductId,
        //            OrderId = OrderInserted.Id,
        //            Count = cart.Count,
        //            Price = cart.Price,
        //        };
        //        var r = _orderDetailRepository.InsertOrderDetail(orderDetail);
        //    }

        //    #region

        //    if (OrderInserted.User.CompanyId.GetValueOrDefault() == 0)
        //    {
        //        // è un cliente regolare e dobbiamo verificare il pagamento
        //        StripeConfiguration.ApiKey = "sk_test_51R7aotGdXWKcYFAs2sZ5ESMHEs5J4HBCI7WtgSbhpIE3MoCMlOg6oXS0h0kSjRmehAF8e3afnsbKEp5y3kbIWGfi00i9yZmmBx";

        //        var domain = "https://localhost:7046/";
        //        var options = new Stripe.Checkout.SessionCreateOptions
        //        {
        //            SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={orderInsertedId}",
        //            CancelUrl = domain + "customer/cart/index",
        //            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
        //            Mode = "payment",
        //        };

        //        foreach (var item in shoppingCartVM.ShoppingCartList)
        //        {
        //            var sessionLineItem = new SessionLineItemOptions
        //            {
        //                PriceData = new SessionLineItemPriceDataOptions
        //                {
        //                    UnitAmount = (long)(item.Price * 100),
        //                    Currency = "eur",
        //                    ProductData = new SessionLineItemPriceDataProductDataOptions
        //                    {
        //                        Name = item.Product.Title,

        //                    }
        //                },
        //                Quantity = item.Count,
        //            };
        //            options.LineItems.Add(sessionLineItem);
        //        }
        //        var service = new Stripe.Checkout.SessionService();
        //        Session session = service.Create(options);
        //        _orderRepository.UpdateStripePaymentID(orderInsertedId, session.Id, session.PaymentIntentId);
        //        Response.Headers.Append("Location", session.Url);
        //        return new StatusCodeResult(303);

        //    }
        //    #endregion

        //    return RedirectToAction(nameof(OrderConfirmation), new { id = OrderInserted.Id});
        //}

        //public IActionResult OrderConfirmation(int id)
        //{
        //    Order order = _orderRepository.GetOrder(id).Result;

        //    #region

        //    if (order.PaymentStatus != SD.PaymentStatusDelayedPayment)
        //    {
        //        var service = new SessionService();
        //        Session session = service.Get(order.SessionId);

        //        if (session.PaymentStatus.ToLower() == "paid")
        //        {
        //            _orderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
        //            _orderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
        //        }
        //    }

        //    #endregion

        //    List<ShoppingCart> shoppingCarts = _shoppingCartRepository.GetShoppingCartsPerUserId(order.UserId).Result.ToList();
        //    foreach (var cart in shoppingCarts)
        //    {
        //        _shoppingCartRepository.DeleteShoppingCart(cart.Id);
        //    }

        //    return View(id);
        //}

        public async Task<IActionResult> Plus(int productId)
        {
            await _shoppingCartService.Plus(productId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int productId)
        {
            await _shoppingCartService.Minus(productId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int productId)
        {
            await _shoppingCartService.Remove(productId);
            return RedirectToAction(nameof(Index));
        }

    }
}
