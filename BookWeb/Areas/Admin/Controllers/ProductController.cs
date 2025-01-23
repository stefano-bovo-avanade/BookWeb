using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository;
using BookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ILogger<HomeController> logger,
            IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
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
        public IActionResult Upsert(ProductVM productVM, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product\");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath + fileName), FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    var productId = _productRepository.InsertProduct(productVM.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _productRepository.UpdateProduct(productVM.Product);
                    TempData["success"] = "Product updated successfully";
                }

                return RedirectToAction("Index");

            }
            else
            {
                var product = productVM.Product;
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

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _productRepository.GetProducts().Result;
            return Json(new 
            {
                data = products,
            });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _productRepository.GetProduct(id).Result;
            if (productToBeDeleted == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Errore nella cancellazione"
                });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _productRepository.DeleteProduct(id);

            return Json(new
            {
                success = true,
                message = "Cancellazione avvenuta con successo",
            });
        }

        #endregion
    }
}
