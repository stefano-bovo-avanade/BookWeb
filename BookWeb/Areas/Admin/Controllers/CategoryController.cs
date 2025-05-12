using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryController(ILogger<HomeController> logger, ICategoryRepository categoryRepository,
            IProductRepository productRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            CategoryVM categoryVM = new CategoryVM();
            try
            {
                categoryVM.CategoryList = _categoryRepository.GetCategories().Result.ToList();
                categoryVM.CategorySelectList = categoryVM.CategoryList.Select(c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(categoryVM);
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
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder can not exactly match the name");
            }
            if (ModelState.IsValid)
            {
                var categoryId = _categoryRepository.InsertCategory(category);
                TempData["success"] = "Category created successfully";
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
            var category = _categoryRepository.GetCategory(Id).Result;
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                var categoryId = _categoryRepository.UpdateCategory(category);
                TempData["success"] = "Category updated successfully";
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
            var category = _categoryRepository.GetCategory(Id).Result;
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? category = _categoryRepository.GetCategory(id).Result;

            if (id == null)
            {
                return NotFound();
            }
            _categoryRepository.DeleteCategory(id);
            TempData["error"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        public JsonResult GetProductByCategoryId(int categoryId) 
        {
            var products = _productRepository.GetProducts().Result;
            var productsByCategoryId = products.Where(p => p.CategoryId == categoryId).ToList();
            return Json(productsByCategoryId);
        }
    }
}
