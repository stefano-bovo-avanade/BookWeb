using BookWeb.Models;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository; 
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _productRepository.GetProducts().Result.ToList();
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            Product product = _productRepository.GetProduct(productId).Result;
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
