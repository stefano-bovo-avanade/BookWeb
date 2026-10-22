using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : BaseController
    {
        private readonly ILogger<CartController> _logger;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        private const string ControllerName = nameof(CartController);

        public CartController(
            ILogger<CartController> logger,
            IShoppingCartService shoppingCartService,
            IUserService userService,
            IOrderService orderService)
        {
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _userService = userService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(Index));

            try
            {
                var cart = await _shoppingCartService.GetOrCreateCartAsync();

                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName}",
                    ControllerName,
                    nameof(Index));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(Index),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int Id, int quantity)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Quantity: {Quantity}",
                ControllerName,
                nameof(AddToCart),
                Id,
                quantity);

            try
            {
                await _shoppingCartService.AddToCartAsync(Id, quantity);

                return RedirectToAction(
                    actionName: "ProductDetail",
                    controllerName: "Home",
                    routeValues: new
                    {
                        area = "Customer",
                        productId = Id
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Quantity: {Quantity}",
                    ControllerName,
                    nameof(AddToCart),
                    Id,
                    quantity);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(AddToCart),
                    Id,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<IActionResult> Summary()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(Summary));

            try
            {
                if (!(User.Identity?.IsAuthenticated ?? false))
                {
                    _logger.LogInformation(
                        "Redirecting unauthenticated user to login. Controller: {ControllerName} | Method: {MethodName}",
                        ControllerName,
                        nameof(Summary));

                    var returnUrl = Url.Action(
                        action: nameof(Summary),
                        controller: "Cart",
                        values: new { area = "Customer" });

                    return RedirectToAction(
                        actionName: "Login",
                        controllerName: "Account",
                        routeValues: new
                        {
                            area = "Customer",
                            returnUrl
                        });
                }

                var cart = await _shoppingCartService.GetOrCreateCartAsync();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = userId != null
                    ? await _userService.GetUserAsync(userId)
                    : null;

                var checkoutVM = new CheckoutVM
                {
                    Items = cart.Items,
                    OrderTotal = cart.CartTotal,
                    City = user?.City,
                    Name = user?.Name,
                    PhoneNumber = user?.PhoneNumber,
                    PostalCode = user?.PostalCode,
                    State = user?.State,
                    StreetAddress = user?.StreetAddress
                };

                return View(checkoutVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName}",
                    ControllerName,
                    nameof(Summary));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(Summary),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST(CheckoutVM checkoutVM)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(SummaryPOST));

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogWarning(
                        "Unauthenticated checkout attempt. Controller: {ControllerName} | Method: {MethodName}",
                        ControllerName,
                        nameof(SummaryPOST));

                    return RedirectToAction(
                        "Login",
                        "Account",
                        new { area = "Customer" });
                }

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
                    Name = checkoutVM.Name
                };

                int orderId;

                try
                {
                    orderId = await _orderService.CreateOrderAsync(userId, order);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Order creation not possible. Controller: {ControllerName} | Method: {MethodName} | UserId: {UserId}",
                        ControllerName,
                        nameof(SummaryPOST),
                        userId);

                    return RedirectToAction(nameof(Index));
                }

                var user = await _userService.GetUserAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning(
                        "User not found after order creation. Controller: {ControllerName} | Method: {MethodName} | UserId: {UserId} | OrderId: {OrderId}",
                        ControllerName,
                        nameof(SummaryPOST),
                        userId,
                        orderId);

                    return RedirectToAction(nameof(Index));
                }

                if (user.CompanyId.GetValueOrDefault() == 0)
                {
                    var sessionUrl = await _orderService.CreateStripeSessionAsync(orderId);

                    if (string.IsNullOrEmpty(sessionUrl))
                    {
                        _logger.LogWarning(
                            "Stripe session URL is empty. Controller: {ControllerName} | Method: {MethodName} | OrderId: {OrderId}",
                            ControllerName,
                            nameof(SummaryPOST),
                            orderId);

                        return RedirectToAction(nameof(Index));
                    }

                    Response.Headers.Append("Location", sessionUrl);

                    return new StatusCodeResult(303);
                }

                return RedirectToAction(
                    nameof(OrderConfirmation),
                    new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName}",
                    ControllerName,
                    nameof(SummaryPOST));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(SummaryPOST),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<IActionResult> OrderConfirmation(int orderId, int cartId)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName} | OrderId: {OrderId} | CartId: {CartId}",
                ControllerName,
                nameof(OrderConfirmation),
                orderId,
                cartId);

            try
            {
                await _shoppingCartService.OrderConfirmation(orderId, cartId);

                return View(orderId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Order confirmation not possible. Controller: {ControllerName} | Method: {MethodName} | OrderId: {OrderId} | CartId: {CartId}",
                    ControllerName,
                    nameof(OrderConfirmation),
                    orderId,
                    cartId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName} | OrderId: {OrderId} | CartId: {CartId}",
                    ControllerName,
                    nameof(OrderConfirmation),
                    orderId,
                    cartId);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | OrderId: {OrderId} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(OrderConfirmation),
                    orderId,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<IActionResult> Plus(int productId)
        {
            return await UpdateCartItemAsync(
                nameof(Plus),
                productId,
                () => _shoppingCartService.Plus(productId));
        }

        public async Task<IActionResult> Minus(int productId)
        {
            return await UpdateCartItemAsync(
                nameof(Minus),
                productId,
                () => _shoppingCartService.Minus(productId));
        }

        public async Task<IActionResult> Remove(int productId)
        {
            return await UpdateCartItemAsync(
                nameof(Remove),
                productId,
                () => _shoppingCartService.Remove(productId));
        }

        private async Task<IActionResult> UpdateCartItemAsync(
            string methodName,
            int productId,
            Func<Task> updateAction)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId}",
                ControllerName,
                methodName,
                productId);

            try
            {
                await updateAction();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId}",
                    ControllerName,
                    methodName,
                    productId);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    methodName,
                    productId,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}