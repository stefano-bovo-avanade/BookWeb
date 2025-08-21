using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderHeaderController : BaseController
    {
        private readonly IOrderHeaderRepository _orderRepository;
        public OrderHeaderController(IOrderHeaderRepository orderRepository) 
        {
            _orderRepository = orderRepository;
        }
        public IActionResult Index()
        {
            var orders = _orderRepository.GetOrderHeaders().Result.ToList();
            return View(orders);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _orderRepository.GetOrderHeaders().Result;
            return Json(new
            {
                data = orders,
            });
        }

        #endregion


    }
}
