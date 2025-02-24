using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public CartController(IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result,
            };
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderTotal += cart.Price;
            }
            return View(shoppingCartVM);
            
        }

        public IActionResult Plus(int cartId) 
        {
            var cartFromDb = _shoppingCartRepository.GetShoppingCart(cartId).Result;
            cartFromDb.Count += 1;
            _shoppingCartRepository.UpdateShoppingCart(cartFromDb);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _shoppingCartRepository.GetShoppingCart(cartId).Result;
            if (cartFromDb.Count == 1)
            {
                _shoppingCartRepository.DeleteShoppingCart(cartId);
            }
            else
            {
                cartFromDb.Count -= 1;
                _shoppingCartRepository.UpdateShoppingCart(cartFromDb);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            _shoppingCartRepository.DeleteShoppingCart(cartId);
            return RedirectToAction(nameof(Index));
        }

        private int GetPriceBasedOnQuantity(ShoppingCart cart) 
        {
            return cart.Count switch
            {
                >= 1 and <= 50 => (int)cart.Product.Price * cart.Count,
                >= 51 and <= 100 => (int)cart.Product.Price50 * cart.Count,
                > 100 => (int)cart.Product.Price100 * cart.Count,
                _ => throw new ArgumentException("Valore non valido"),
            };
        }
    }
}
