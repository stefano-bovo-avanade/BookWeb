using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ILogger _logger;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ILogger<HomeController> logger, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var categories = _categoryRepository.GetCategories().Result;
                return View(categories);
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
    }
}
