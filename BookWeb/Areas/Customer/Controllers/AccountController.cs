using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Context;
using BookWeb.Service.Service.IService;
using BookWeb.Utility.Authentication;
using BookWeb.Utility.ShoppingCartHelper;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly JwtService _jwtService;
        private readonly CartHelper _cartHelper;

        public AccountController(
            IConfiguration config,
            ApplicationDbContext context,
            IMapper mapper,
            IShoppingCartService shoppingCartService,
            JwtService jwtService,
            CartHelper cartHelper)
        {
            _config = config;
            _context = context;
            _mapper = mapper;
            _shoppingCartService = shoppingCartService;
            _jwtService = jwtService;
            _cartHelper = cartHelper;
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

            var user = new User();
            var hasher = new PasswordHasher<User>();
            user = _mapper.From(inputUser).AdaptToType<User>();
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = hasher.HashPassword(user, inputUser.Password);
            user.Role = _context.Roles.Where(r => r.Id == inputUser.RoleId).FirstOrDefault()!;


            // Login automatico dopo registrazione

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
        LoginRequest request,
        string returnUrl = null)
        {
            if (!string.IsNullOrWhiteSpace(request.LoginTicket))
            {
                return await LoginWithTicketAsync(request, returnUrl);
            }

            return await LoginWithCredentialsAsync(request, returnUrl);
        }


        private async Task<IActionResult> LoginWithTicketAsync(
        LoginRequest request,
        string returnUrl)
        {
            var pendingLogin = await _context.PendingLogins
                .FirstOrDefaultAsync(p => p.Ticket == request.LoginTicket);

            if (pendingLogin == null ||
                pendingLogin.ExpiresAt <= DateTime.UtcNow)
            {
                if (pendingLogin != null)
                {
                    _context.PendingLogins.Remove(pendingLogin);
                    await _context.SaveChangesAsync();
                }

                ViewBag.Error = "Sessione di login scaduta. Accedi nuovamente.";
                return View("Login", new LoginRequest());
            }

            var allowedRoleIds = pendingLogin.AllowedRoleIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            if (request.RoleId == null ||
                !allowedRoleIds.Contains(request.RoleId.Value))
            {
                ViewBag.Error = request.RoleId == null
                    ? "Seleziona un ruolo."
                    : "Ruolo selezionato non valido.";

                ViewBag.Roles = await _context.Roles
                    .Where(r => allowedRoleIds.Contains(r.Id))
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return View("Login", request);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == pendingLogin.UserId);

            if (user == null)
            {
                _context.PendingLogins.Remove(pendingLogin);
                await _context.SaveChangesAsync();

                ViewBag.Error = "Utente non trovato. Accedi nuovamente.";
                return View("Login", new LoginRequest());
            }

            var selectedRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId.Value);

            if (selectedRole == null)
            {
                ViewBag.Error = "Ruolo non valido.";

                ViewBag.Roles = await _context.Roles
                    .Where(r => allowedRoleIds.Contains(r.Id))
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return View("Login", request);
            }

            var originalRoles = user.UserRoles
                .Select(ur => ur.Role)
                .ToList();

            if (originalRoles.Count == 0)
            {
                _context.PendingLogins.Remove(pendingLogin);
                await _context.SaveChangesAsync();

                ViewBag.Error = "Utente senza ruoli assegnati.";
                return View("Login", new LoginRequest());
            }

            var originalRole = GetOriginalRole(originalRoles);

            if (originalRole == null)
            {
                ViewBag.Error = "Impossibile determinare il ruolo originale.";
                return View("Login", new LoginRequest());
            }

            _context.PendingLogins.Remove(pendingLogin);
            await _context.SaveChangesAsync();

            return await CompleteLoginAsync(
                user.Id,
                originalRole,
                selectedRole.Name,
                returnUrl);
        }

        private async Task<IActionResult> LoginWithCredentialsAsync(
        LoginRequest request,
        string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                ViewBag.Error = "Username e password obbligatori.";
                return View("Login", request);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                ViewBag.Error = "Credenziali errate.";
                return View("Login", request);
            }

            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Credenziali errate.";
                return View("Login", request);
            }

            var originalRoles = user.UserRoles
                .Select(ur => ur.Role)
                .ToList();

            if (originalRoles.Count == 0)
            {
                ViewBag.Error = "Utente senza ruoli assegnati.";
                return View("Login", request);
            }

            var availableRoles = await GetAvailableRolesAsync(originalRoles);

            if (availableRoles.Count == 0)
            {
                ViewBag.Error = "Non sono disponibili ruoli per questo utente.";
                return View("Login", request);
            }

            if (availableRoles.Count > 1 && request.RoleId == null)
            {
                return await CreatePendingLoginAsync(
                    user.Id,
                    availableRoles,
                    request);
            }

            var selectedRole = availableRoles
                .FirstOrDefault(r => r.Id == request.RoleId);

            if (selectedRole == null)
            {
                ViewBag.Error = "Ruolo selezionato non valido.";
                ViewBag.Roles = availableRoles;

                return View("Login", request);
            }

            var originalRole = GetOriginalRole(originalRoles);

            if (originalRole == null)
            {
                ViewBag.Error = "Impossibile determinare il ruolo originale.";
                return View("Login", request);
            }

            return await CompleteLoginAsync(
                user.Id,
                originalRole,
                selectedRole.Name,
                returnUrl);
        }

        private async Task<List<Role>> GetAvailableRolesAsync(
        List<Role> originalRoles)
        {
            var isAdmin = originalRoles.Any(r => r.Name == "Admin");
            var isEmployee = originalRoles.Any(r => r.Name == "Employee");

            if (isAdmin)
            {
                return await _context.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }

            if (isEmployee)
            {
                return await _context.Roles
                    .Where(r => r.Name == "Employee"
                             || r.Name == "Customer"
                             || r.Name == "Company")
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }

            return originalRoles;
        }

        private string GetOriginalRole(List<Role> roles)
        {
            return roles.FirstOrDefault(r => r.Name == "Admin")?.Name
                ?? roles.FirstOrDefault(r => r.Name == "Employee")?.Name
                ?? roles.FirstOrDefault()?.Name;
        }

        private async Task<IActionResult> CreatePendingLoginAsync(
        string userId,
        List<Role> availableRoles,
        LoginRequest request)
        {
            var previousTickets = await _context.PendingLogins
                .Where(p => p.UserId == userId)
                .ToListAsync();

            _context.PendingLogins.RemoveRange(previousTickets);

            var ticket = Convert.ToHexString(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

            var pendingLogin = new PendingLogin
            {
                Ticket = ticket,
                UserId = userId,
                AllowedRoleIds = string.Join(
                    ",",
                    availableRoles.Select(r => r.Id)),
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            _context.PendingLogins.Add(pendingLogin);
            await _context.SaveChangesAsync();

            request.LoginTicket = ticket;

            ViewBag.Roles = availableRoles;

            return View("Login", request);
        }

        private async Task<IActionResult> CompleteLoginAsync(
        string userId,
        string originalRole,
        string selectedRole,
        string returnUrl)
        {
            var token = _jwtService.GenerateToken(
                userId,
                originalRole,
                selectedRole);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Unisce il carrello ospite al carrello dell'utente.
            await MergeGuestCartAfterLoginAsync(userId);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

        private async Task MergeGuestCartAfterLoginAsync(string userId)
        {
            // Recupera il token del carrello ospite dal cookie.
            var guestCartToken = _cartHelper.GetGuestCartToken();

            // Se non esiste un carrello ospite, non c'è nulla da unire.
            if (string.IsNullOrWhiteSpace(guestCartToken))
                return;

            // Unisce il carrello ospite a quello dell'utente autenticato.
            await _shoppingCartService.MergeGuestCartAsync(
                guestCartToken,
                userId);

            // Cancella il cookie soltanto dopo il merge riuscito.
            _cartHelper.ClearGuestCartToken();
        }
    }
}
