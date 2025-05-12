using BookWeb.Authentication;
using BookWeb.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        public AccountController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register() => View(new InputUser());

        [HttpPost]
        public async Task<IActionResult> Register(InputUser inputUser)
        {
            if (string.IsNullOrWhiteSpace(inputUser.UserName) || string.IsNullOrWhiteSpace(inputUser.Password))
            {
                ViewBag.Error = "Username e password obbligatori";
                return View();
            }

            var existing = await _authService.UserExists(inputUser.UserName);
            if (existing)
            {
                ViewBag.Error = "Utente già esistente";
                return View();
            }

            var user = new ApplicationUser { UserName = inputUser.UserName };
            var hasher = new PasswordHasher<ApplicationUser>();
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = hasher.HashPassword(user, inputUser.Password);
            user.UserName = inputUser.UserName;
            user.Name = inputUser.UserName;
            user.Email = inputUser.Email;
            user.StreetAddress = inputUser.StreetAddress;
            user.PhoneNumber = inputUser.PhoneNumber;
            user.City = inputUser.City;
            user.State = inputUser.State;
            user.PostalCode = inputUser.PostalCode;
            user.Role = inputUser.Role;


            await _authService.Register(user);

            // Login automatico dopo registrazione
            var token = JwtManager.GenerateToken(user, _config);
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(InputUser utente)
        {
            var user = await _authService.Authenticate(utente.UserName, utente.Password);

            if (user != null)
            {
                var token = JwtManager.GenerateToken(user, _config);

                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenziali errate";
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }
    }
}
