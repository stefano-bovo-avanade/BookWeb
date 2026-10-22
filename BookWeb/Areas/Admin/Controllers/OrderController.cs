using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly ILogger<OrderController> _logger;

        private const string ControllerName = nameof(OrderController);

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _logger = logger;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Index));

            try
            {
                return await Task.FromResult<IActionResult>(View());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Index));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Index),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Details));

            try
            {
                OrderVM = new OrderVM
                {
                    Order = await _orderService.GetOrderAsync(orderId),
                    OrderDetailList =
                        await _orderDetailService.GetOrderDetailsPerOrderId(orderId)
                };

                if (OrderVM.Order == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | OrderId: {OrderId} | Motivo: ordine non trovato",
                        ControllerName,
                        nameof(Details),
                        orderId);

                    return NotFound();
                }

                return View(OrderVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method} | OrderId: {OrderId}",
                    ControllerName,
                    nameof(Details),
                    orderId);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Details),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // UPDATE ORDER DETAIL
        // =========================================================

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(UpdateOrderDetail));

            try
            {
                if (OrderVM?.Order == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: dati ordine mancanti",
                        ControllerName,
                        nameof(UpdateOrderDetail));

                    TempData["Error"] = "Dati dell'ordine non validi.";

                    return RedirectToAction(nameof(Index));
                }

                var orderFromDb = await _orderService.GetOrderAsync(
                    OrderVM.Order.Id);

                if (orderFromDb == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | OrderId: {OrderId} | Motivo: ordine non trovato",
                        ControllerName,
                        nameof(UpdateOrderDetail),
                        OrderVM.Order.Id);

                    return NotFound();
                }

                orderFromDb.Name = OrderVM.Order.Name;
                orderFromDb.PhoneNumber = OrderVM.Order.PhoneNumber;
                orderFromDb.StreetAddress = OrderVM.Order.StreetAddress;
                orderFromDb.City = OrderVM.Order.City;
                orderFromDb.State = OrderVM.Order.State;
                orderFromDb.PostalCode = OrderVM.Order.PostalCode;

                if (!string.IsNullOrEmpty(OrderVM.Order.Carrier))
                {
                    orderFromDb.Carrier = OrderVM.Order.Carrier;
                }

                if (!string.IsNullOrEmpty(OrderVM.Order.TrackingNumber))
                {
                    orderFromDb.TrackingNumber = OrderVM.Order.TrackingNumber;
                }

                await _orderService.UpdateOrder(orderFromDb);

                _logger.LogInformation(
                    "Ordine aggiornato correttamente | Controller: {Controller} | Method: {Method} | OrderId: {OrderId}",
                    ControllerName,
                    nameof(UpdateOrderDetail),
                    orderFromDb.Id);

                TempData["Success"] =
                    "Dettagli ordini aggiornati con successo";

                return RedirectToAction(
                    nameof(Details),
                    new { orderId = orderFromDb.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(UpdateOrderDetail));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(UpdateOrderDetail),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // START PROCESSING
        // =========================================================

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(StartProcessing));

            try
            {
                if (OrderVM?.Order == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: dati ordine mancanti",
                        ControllerName,
                        nameof(StartProcessing));

                    TempData["Error"] = "Dati dell'ordine non validi.";

                    return RedirectToAction(nameof(Index));
                }

                var orderId = OrderVM.Order.Id;

                await _orderService.UpdateOrderStatus(
                    orderId,
                    SD.StatusInProcess);

                _logger.LogInformation(
                    "Stato ordine aggiornato | Controller: {Controller} | Method: {Method} | OrderId: {OrderId} | Status: {Status}",
                    ControllerName,
                    nameof(StartProcessing),
                    orderId,
                    SD.StatusInProcess);

                TempData["Success"] =
                    "Dettagli ordini aggiornati correttamente";

                return RedirectToAction(
                    nameof(Details),
                    new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(StartProcessing));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(StartProcessing),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        #region API CALLS

        // =========================================================
        // GET ALL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(GetAll));

            try
            {
                var isAuthenticated =
                    User.Identity?.IsAuthenticated ?? false;

                var userId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation(
                    "Richiesta ordini | Controller: {Controller} | Method: {Method} | Authenticated: {Authenticated} | UserId: {UserId} | Status: {Status}",
                    ControllerName,
                    nameof(GetAll),
                    isAuthenticated,
                    userId,
                    status);

                IEnumerable<Order> orders;

                if (User.IsInRole(SD.Role_Admin) ||
                    User.IsInRole(SD.Role_Employee))
                {
                    orders = await _orderService.GetOrdersAsync();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        _logger.LogWarning(
                            "WARNING | Controller: {Controller} | Method: {Method} | Motivo: utente non autenticato o UserId mancante",
                            ControllerName,
                            nameof(GetAll));

                        return Unauthorized();
                    }

                    orders = await _orderService.GetOrderPerUserIdAsync(
                        userId);
                }

                switch (status)
                {
                    case "inprocess":
                        orders = orders.Where(
                            o => o.OrderStatus == SD.StatusInProcess);
                        break;

                    case "pending":
                        orders = orders.Where(
                            o => o.PaymentStatus ==
                                 SD.PaymentStatusDelayedPayment);
                        break;

                    case "completed":
                        orders = orders.Where(
                            o => o.PaymentStatus == SD.StatusShipped);
                        break;

                    case "approved":
                        orders = orders.Where(
                            o => o.OrderStatus == SD.StatusApproved);
                        break;

                    default:
                        break;
                }

                var orderList = orders.ToList();

                _logger.LogInformation(
                    "Ordini recuperati | Controller: {Controller} | Method: {Method} | Count: {Count}",
                    ControllerName,
                    nameof(GetAll),
                    orderList.Count);

                return Json(new
                {
                    data = orderList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method} | Status: {Status}",
                    ControllerName,
                    nameof(GetAll),
                    status);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(GetAll),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        #endregion
    }
}