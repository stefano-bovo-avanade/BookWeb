using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _logger = logger;
        }

        // GET: /admin/product
        public IActionResult Index()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                nameof(ProductController),
                nameof(Index));

            try
            {
                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    nameof(ProductController),
                    nameof(Index),
                    stopwatch.ElapsedMilliseconds);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    nameof(ProductController),
                    nameof(Index));

                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // GET: /admin/product/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                nameof(ProductController),
                nameof(GetAll));

            try
            {
                var products = await _productRepository.GetProducts();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    nameof(ProductController),
                    nameof(GetAll),
                    stopwatch.ElapsedMilliseconds);

                return Json(new
                {
                    data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    nameof(ProductController),
                    nameof(GetAll));

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Errore durante il recupero dei prodotti."
                    });
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // GET: /admin/product/upsert?id=5
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                nameof(ProductController),
                nameof(Upsert),
                id);

            try
            {
                Product product;

                if (id.HasValue && id.Value > 0)
                {
                    product = await _productRepository.GetProduct(id);

                    if (product == null)
                    {
                        _logger.LogWarning(
                            "WARNING | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Prodotto non trovato",
                            nameof(ProductController),
                            nameof(Upsert),
                            id);

                        return NotFound();
                    }
                }
                else
                {
                    product = new Product();
                }

                await LoadCategories(product.CategoryId);

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Duration: {Duration} ms",
                    nameof(ProductController),
                    nameof(Upsert),
                    id,
                    stopwatch.ElapsedMilliseconds);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                    nameof(ProductController),
                    nameof(Upsert),
                    id);

                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // POST: /admin/product/upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Product product)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                nameof(ProductController),
                nameof(Upsert),
                product.Id);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | ModelState non valido | ProductId: {ProductId}",
                        nameof(ProductController),
                        nameof(Upsert),
                        product.Id);

                    await LoadCategories(product.CategoryId);

                    return View(product);
                }

                if (product.Id == 0)
                {
                    var newId = await _productRepository.InsertProduct(product);

                    if (newId <= 0)
                    {
                        _logger.LogWarning(
                            "WARNING | Controller: {Controller} | Method: {Method} | Inserimento non riuscito",
                            nameof(ProductController),
                            nameof(Upsert));

                        ModelState.AddModelError(
                            string.Empty,
                            "Non è stato possibile creare il prodotto.");

                        await LoadCategories(product.CategoryId);

                        return View(product);
                    }

                    _logger.LogInformation(
                        "Prodotto creato | ProductId: {ProductId} | Title: {Title}",
                        newId,
                        product.Title);

                    TempData["success"] = "Prodotto creato con successo.";
                }
                else
                {
                    var rowsAffected = await _productRepository.UpdateProduct(product);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning(
                            "WARNING | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Nessun prodotto aggiornato",
                            nameof(ProductController),
                            nameof(Upsert),
                            product.Id);

                        ModelState.AddModelError(
                            string.Empty,
                            "Il prodotto non è stato trovato oppure non è stato aggiornato.");

                        await LoadCategories(product.CategoryId);

                        return View(product);
                    }

                    _logger.LogInformation(
                        "Prodotto aggiornato | ProductId: {ProductId} | Title: {Title}",
                        product.Id,
                        product.Title);

                    TempData["success"] = "Prodotto aggiornato con successo.";
                }

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Duration: {Duration} ms",
                    nameof(ProductController),
                    nameof(Upsert),
                    product.Id,
                    stopwatch.ElapsedMilliseconds);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                    nameof(ProductController),
                    nameof(Upsert),
                    product.Id);

                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // DELETE: /admin/product/delete/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                nameof(ProductController),
                nameof(Delete),
                id);

            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        message = "ID prodotto non valido."
                    });
                }

                var rowsAffected = await _productRepository.DeleteProduct(id);

                if (rowsAffected == 0)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Prodotto non trovato",
                        nameof(ProductController),
                        nameof(Delete),
                        id);

                    return NotFound(new
                    {
                        message = "Prodotto non trovato."
                    });
                }

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | ProductId: {ProductId} | Duration: {Duration} ms",
                    nameof(ProductController),
                    nameof(Delete),
                    id,
                    stopwatch.ElapsedMilliseconds);

                return Json(new
                {
                    success = true,
                    message = "Prodotto eliminato con successo."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method} | ProductId: {ProductId}",
                    nameof(ProductController),
                    nameof(Delete),
                    id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Errore durante l'eliminazione del prodotto."
                    });
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // Carica le categorie per il menu a tendina
        private async Task LoadCategories(int? selectedCategoryId)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.CategoryList = new SelectList(
                categories,
                "Id",
                "Name",
                selectedCategoryId);
        }
    }
}