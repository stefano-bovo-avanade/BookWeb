using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : BaseController
    {
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        
        public CartController(IShoppingCartService shoppingCartService, 
            IUserService userService,
            IOrderService orderService)
        {
            _shoppingCartService = shoppingCartService;
            _userService = userService;
            _orderService = orderService;
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

            // Recupera o crea il carrello per l'utente loggato
            var cart = await _shoppingCartService.GetOrCreateCartAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId != null ? await _userService.GetUserAsync(userId) : null;

            CheckoutVM checkoutVM = new CheckoutVM()
            {
                Items = cart.Items,
                OrderTotal = cart.CartTotal,
                City = user?.City,
                Name = user?.Name,
                PhoneNumber = user?.PhoneNumber,
                PostalCode = user?.PostalCode,
                State = user?.State,
                StreetAddress = user?.StreetAddress,
            };

            return View(checkoutVM);
        }


        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST(CheckoutVM checkoutVM)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var cart = await _shoppingCartService.GetOrCreateCartAsync();

            int orderId;
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(7),
                OrderTotal = 0,
                City = checkoutVM.City,
                PhoneNumber = checkoutVM.PhoneNumber,
                PostalCode = checkoutVM.PostalCode,
                State = checkoutVM.State,
                StreetAddress = checkoutVM.StreetAddress,
                Name = checkoutVM.Name,
            };

            try
            {
                orderId = await _orderService.CreateOrderAsync(userId, order);
            }
            catch (InvalidOperationException)
            {
                // Es. carrello vuoto
                return RedirectToAction("Index");
            }

            var user = await _userService.GetUserAsync(userId);


            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                var sessionUrl = await _orderService.CreateStripeSessionAsync(orderId);

                if (string.IsNullOrEmpty(sessionUrl))
                    return RedirectToAction("Index");

                // Reindirizza alla checkout Stripe
                Response.Headers.Append("Location", sessionUrl);
                return new StatusCodeResult(303);
            }

            // Se azienda (pagamento posticipato), vai direttamente alla conferma ordine
            return RedirectToAction(nameof(OrderConfirmation), new { id = orderId });
        }

        public async Task<IActionResult> OrderConfirmation(int orderId, int cartId)
        {
            try
            {
                await _shoppingCartService.OrderConfirmation(orderId, cartId);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("Index");
            }

            return View(orderId);
        }

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
