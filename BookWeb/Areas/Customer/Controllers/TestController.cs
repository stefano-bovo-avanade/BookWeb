using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TestController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public TestController(IProductRepository productRepository, ICategoryRepository categoryRepository,
            ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _context = context;
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

        public async Task<JsonResult> TestApi()
        {
            DateTime start = new DateTime(2025, 06, 01);
            DateTime end = new DateTime(2025, 06, 11);

            return Json(new
            {
                
            });
        }
    }
    class PetOwner
    {
        public string Name { get; set; }
        public List<string> Pets { get; set; }
    }
}
