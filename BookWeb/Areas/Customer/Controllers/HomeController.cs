using BookWeb.Authentication;
using BookWeb.Models;
using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository,
            ICategoryRepository categoryRepository, IShoppingCartRepository shoppingCartRepository, 
            IConfiguration configuration)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _config = configuration;

        }

        public IActionResult Index()
        {
            IEnumerable<VProduct> productList = _productRepository.GetProducts().Result.ToList();


            ViewBag.Username = User!.Identity!.Name;

            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart shoppingCart = new ShoppingCart() 
            { 
                Product = _productRepository.GetProduct(productId).Result,
                Count = 1,
                ProductId = productId
            };
            return View(shoppingCart);
        }

        [HttpPost]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            if (!JwtHelper.IsUserLoggedIn(HttpContext, _config))
            {
                return RedirectToAction("Login", "Account");
            }

            var token = Request.Cookies["jwt"];
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var username = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            shoppingCart.ApplicationUserId = userId;
            
            List<ShoppingCart> shoppingCartsPerId = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result.ToList();
            foreach (ShoppingCart item in shoppingCartsPerId)
            {
                if (item.ProductId == shoppingCart.ProductId)
                {
                    item.Count += shoppingCart.Count;
                    _shoppingCartRepository.UpdateShoppingCart(item);
                    return RedirectToAction(nameof(Index));
                }
            }
            _shoppingCartRepository.InsertShoppingCart(shoppingCart);
            return RedirectToAction(nameof(Index));
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
