using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TestController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public TestController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var categories = _categoryRepository.GetCategories().Result;
            var products = new List<Product>();

            ViewBag.Products = new SelectList(products, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");


            return View();
        }

        public JsonResult GetProductByCategoryId(int categoryId)
        {
            var products = _productRepository.GetProducts().Result;
            var productsByCategoryId = products.Where(p => p.CategoryId == categoryId).ToList();
            return Json(productsByCategoryId);
        }
    }
}
