using BookWeb.Models;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookWeb.Areas.Customer.Controllers
{
    
    [Area("Customer")]
    public class HomeController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository,
            ICategoryRepository categoryRepository, IShoppingCartRepository shoppingCartRepository, 
            IConfiguration configuration)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _config = configuration;

        }

        public IActionResult Index()
        {
            IEnumerable<VProduct> productList = _productRepository.GetProducts().Result.ToList();
            ViewBag.Username = User!.Identity!.Name;
            return View(productList);
        }

        public IActionResult ProductDetail(int productId)
        {
            Product product = _productRepository.GetProduct(productId).Result;

            if (product == null)
            {
                return NotFound();
            }

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
