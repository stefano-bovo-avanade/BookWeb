using BookWeb.Areas.Customer.Controllers;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ILogger<HomeController> logger, ICompanyRepository companyRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _companyRepository = companyRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var companies = _companyRepository.GetCompanies().Result;
                return View(companies);
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
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                var company = _companyRepository.GetCompany(id).Result;
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (!ModelState.IsValid)
            {
                return View(company);
            }

            if (company.Id == 0)
            {
                _companyRepository.InsertCompany(company);
                TempData["success"] = "Product created successfully";
            }
            else
            {
                _companyRepository.UpdateCompany(company);
                TempData["success"] = "Product updated successfully";
            }
            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _companyRepository.GetCompanies().Result;
            return Json(new
            {
                data = companies,
            });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _companyRepository.GetCompany(id).Result;
            if (companyToBeDeleted == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Errore nella cancellazione"
                });
            }

            _companyRepository.DeleteCompany(id);

            return Json(new
            {
                success = true,
                message = "Cancellazione avvenuta con successo",
            });
        }

        #endregion
    }
}
