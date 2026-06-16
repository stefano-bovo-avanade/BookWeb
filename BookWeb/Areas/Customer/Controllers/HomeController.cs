using BookWeb.Features.Products.Queries;
using BookWeb.Models;
using BookWeb.Repository.Repository.IRepository;
using MediatR;
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
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository,
            ICategoryRepository categoryRepository, IShoppingCartRepository shoppingCartRepository, 
            IConfiguration configuration, IMediator mediator)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _mediator = mediator;
            _config = configuration;

        }

        public async Task<IActionResult> Index()
        {
            var productList = await _productRepository.GetProducts();
            ViewBag.Username = User?.Identity?.Name ?? "Guest";
            return View(productList);
        }

        public async Task<IActionResult> ProductDetail(int productId)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(productId));

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
