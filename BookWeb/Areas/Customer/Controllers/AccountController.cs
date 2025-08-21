using BookWeb.Common.Authentication;
using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service;
using BookWeb.Service.Context;
using BookWeb.Utility.Authentication;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IShoppingCartService _shoppingCartService;

        public AccountController(AuthService authService, IConfiguration config, 
            ApplicationDbContext context, IMapper mapper, IShoppingCartService shoppingCartService)
        {
            _authService = authService;
            _config = config;
            _context = context;
            _mapper = mapper;
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roles = _context.Roles.ToList();
            var companies = _context.Companies.ToList();
            UserVM userVM = new UserVM()
            {
                InputUser = new(),
                Companies = new SelectList(companies, "Id", "Name"),
            };
            if (User.IsInRole("Admin"))
            {
                userVM.Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name,
                });
            }
            else
            {
                userVM.Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name,
                }).Where(i => i.Text != "Admin");
            }
                //ViewBag.Roles = new SelectList(categories, "Id", "Name");
                return View(userVM);
        }

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

            var user = new User();
            var hasher = new PasswordHasher<User>();
            user = _mapper.From(inputUser).AdaptToType<User>();
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = hasher.HashPassword(user, inputUser.Password);
            user.Role = _context.Roles.Where(r => r.Id == inputUser.RoleId).FirstOrDefault()!;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(InputUser utente, string returnUrl = null)
        {
            var user = await _authService.Authenticate(utente.UserName, utente.Password);

            if (user != null)
            {
                // Genera JWT e salva nei cookie
                var token = JwtManager.GenerateToken(user, _config);

                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // MERGE del carrello guest
                var guestCartToken = Request.Cookies["cartToken"];

                if (!string.IsNullOrEmpty(guestCartToken) && guestCartToken != user.Id)
                {
                    await _shoppingCartService.MergeGuestCartAsync(guestCartToken, user.Id);

                    // Elimina il cookie del guest cart dopo il merge
                    Response.Cookies.Delete("cartToken");
                }

                // Redirect
                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
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
