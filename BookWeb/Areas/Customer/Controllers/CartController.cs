using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : BaseController
    {
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;

        public CartController(IShoppingCartRepository shoppingCartRepository, IApplicationUserRepository applicationUserRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _applicationUserRepository = applicationUserRepository;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result,
                OrderHeader = new(),
            };
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
            }
            return View(shoppingCartVM);
            
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _shoppingCartRepository.GetShoppingCartsPerUserId(userId).Result,
                OrderHeader = new(),
            };

            shoppingCartVM.OrderHeader.ApplicationUser  = _applicationUserRepository.GetApplicationUser(userId).Result;

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price;
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
