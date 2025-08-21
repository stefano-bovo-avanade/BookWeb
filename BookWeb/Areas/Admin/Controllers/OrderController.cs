using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IOrderRepository orderRepository, 
            IOrderDetailRepository orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int orderId)
        {
            this.OrderVM = new OrderVM
            {
                Order = _orderRepository.GetOrder(orderId).Result,
                OrderDetailList = _orderDetailRepository.GetOrderDetailsPerOrderId(orderId).Result,
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderFromDb = _orderRepository.GetOrder(OrderVM.Order.Id).Result;
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

            _orderRepository.UpdateOrder(orderFromDb);
            TempData["Success"] = "Dettagli ordini aggiornati con successo";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Order> orders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orders = _orderRepository.GetOrders().Result;
            }
            else 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                orders = _orderRepository.GetOrderPerUserId(userId).Result;
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
