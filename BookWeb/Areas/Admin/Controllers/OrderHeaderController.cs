using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderHeaderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        public OrderHeaderController(IOrderHeaderRepository orderHeaderRepository) 
        {
            _orderHeaderRepository = orderHeaderRepository;
        }
        public IActionResult Index()
        {
            var orderHeaders = _orderHeaderRepository.GetOrderHeaders();
            return View(orderHeaders);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var orderHeaders = _orderHeaderRepository.GetOrderHeaders().Result;
            return Json(new
            {
                data = orderHeaders,
            });
        }

        #endregion


    }
}
