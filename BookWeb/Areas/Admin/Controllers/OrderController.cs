using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IOrderService orderService, 
            IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            this.OrderVM = new OrderVM
            {
                Order = await _orderService.GetOrderAsync(orderId),
                OrderDetailList = await _orderDetailService.GetOrderDetailsPerOrderId(orderId),
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderFromDb = _orderService.GetOrderAsync(OrderVM.Order.Id).Result;
            orderFromDb.Name = OrderVM.Order.Name;
            orderFromDb.PhoneNumber = OrderVM.Order.PhoneNumber;
            orderFromDb.StreetAddress = OrderVM.Order.StreetAddress;
            orderFromDb.City = OrderVM.Order.City;
            orderFromDb.State = OrderVM.Order.State;
            orderFromDb.PostalCode = OrderVM.Order.PostalCode;
            if (!string.IsNullOrEmpty(orderFromDb.Carrier))
            {
                orderFromDb.Carrier = orderFromDb.Carrier;
            }
            if (!string.IsNullOrEmpty(orderFromDb.TrackingNumber))
            {
                orderFromDb.Carrier = orderFromDb.TrackingNumber;
            }

            await _orderService.UpdateOrder(orderFromDb);
            TempData["Success"] = "Dettagli ordini aggiornati con successo";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            await _orderService.UpdateOrderStatus(OrderVM.Order.Id, SD.StatusInProcess);
            TempData["Success"] = "Dettagli ordini aggiornati correttamente";
            return RedirectToAction(nameof(Details), new { orderid = OrderVM.Order.Id });
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<Order> orders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orders = await _orderService.GetOrdersAsync();
            }
            else 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                orders = await _orderService.GetOrderPerUserIdAsync(userId);
            }

            switch (status)
            {
                case "inprocess":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "pending":
                    orders = orders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "completed":
                    orders = orders.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new
            {
                data = orders,
            });
        }

        #endregion
    }
}
