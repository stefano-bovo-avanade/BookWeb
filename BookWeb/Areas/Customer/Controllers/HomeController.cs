using BookWeb.Models;
using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var productList = await _productRepository.GetProducts();

            ViewBag.Username = User?.Identity?.Name ?? "Guest";

            return View(productList);
        }

        public async Task<IActionResult> ProductDetail(int productId)
        {
            _logger.LogInformation(
                "Getting Product with Id: {ProductId}",
                productId);

            var product = await _productRepository.GetProduct(productId);

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

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}