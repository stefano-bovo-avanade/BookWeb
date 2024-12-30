using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;

        public ProductController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var products = _productRepository.GetProducts().Result;
                return View(products);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var productId = _productRepository.InsertProduct(product);
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var product = _productRepository.GetProduct(Id).Result;
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var productId = _productRepository.UpdateProduct(product);
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var product = _productRepository.GetProduct(Id).Result;
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? product = _productRepository.GetProduct(id).Result;

            if (id == null)
            {
                return NotFound();
            }
            _productRepository.DeleteProduct(id);
            TempData["error"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
