using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(ILogger<HomeController> logger, 
            IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
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
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> categoryList = _categoryRepository.GetCategories().Result.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ViewBag.CategoryList = categoryList;
            ProductVM productVM = new()
            {
                CategoryList = categoryList,
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _productRepository.GetProduct(id).Result;
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var productId = _productRepository.InsertProduct(productVM.Product);
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else 
            {
                productVM.CategoryList = _categoryRepository.GetCategories().Result.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(productVM);
            }
        }

        //[HttpGet]
        //public IActionResult Edit(int? Id)
        //{
        //    if (Id == null || Id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var product = _productRepository.GetProduct(Id).Result;
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var productId = _productRepository.UpdateProduct(product);
        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

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
