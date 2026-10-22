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

        private const string ControllerName = nameof(HomeController);

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(Index));

            try
            {
                var productList = await _productRepository.GetProducts();

                ViewBag.Username = User?.Identity?.Name ?? "Guest";

                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName}",
                    ControllerName,
                    nameof(Index));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(Index),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<IActionResult> ProductDetail(int productId)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId}",
                ControllerName,
                nameof(ProductDetail),
                productId);

            try
            {
                var product = await _productRepository.GetProduct(productId);

                if (product == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Product not found",
                        ControllerName,
                        nameof(ProductDetail),
                        productId);

                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId}",
                    ControllerName,
                    nameof(ProductDetail),
                    productId);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | ProductId: {ProductId} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(ProductDetail),
                    productId,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public IActionResult Privacy()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(Privacy));

            try
            {
                return View();
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(Privacy),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {ControllerName} | Method: {MethodName}",
                ControllerName,
                nameof(Error));

            try
            {
                return View(new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {ControllerName} | Method: {MethodName} | Duration: {ElapsedMilliseconds} ms",
                    ControllerName,
                    nameof(Error),
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}