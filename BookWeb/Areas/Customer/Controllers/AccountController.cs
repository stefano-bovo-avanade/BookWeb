using System.Diagnostics;
using System.Security.Cryptography;
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
        private readonly ILogger<AccountController> _logger;

        private const string ControllerName = nameof(AccountController);

        public AccountController(
            IConfiguration config,
            ApplicationDbContext context,
            IMapper mapper,
            IShoppingCartService shoppingCartService,
            JwtService jwtService,
            CartHelper cartHelper,
            ILogger<AccountController> logger)
        {
            _config = config;
            _context = context;
            _mapper = mapper;
            _shoppingCartService = shoppingCartService;
            _jwtService = jwtService;
            _cartHelper = cartHelper;
            _logger = logger;
        }

        // =========================================================
        // REGISTER GET
        // =========================================================

        [HttpGet]
        public IActionResult Register()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Register));

            try
            {
                var roles = _context.Roles.ToList();
                var companies = _context.Companies.ToList();

                UserVM userVM = new UserVM
                {
                    InputUser = new(),
                    Companies = new SelectList(
                        companies,
                        "Id",
                        "Name")
                };

                if (User.IsInRole("Admin"))
                {
                    userVM.Roles = roles.Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    });
                }
                else
                {
                    userVM.Roles = roles
                        .Select(r => new SelectListItem
                        {
                            Value = r.Id.ToString(),
                            Text = r.Name
                        })
                        .Where(i => i.Text != "Admin");
                }

                return View(userVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Register));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Register),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // REGISTER POST
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Register(InputUser inputUser)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Register));

            try
            {
                if (string.IsNullOrWhiteSpace(inputUser.UserName) ||
                    string.IsNullOrWhiteSpace(inputUser.Password))
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: username o password mancanti",
                        ControllerName,
                        nameof(Register));

                    ViewBag.Error = "Username e password obbligatori";

                    return View();
                }

                var hasher = new PasswordHasher<User>();

                var user = _mapper
                    .From(inputUser)
                    .AdaptToType<User>();

                user.Id = Guid.NewGuid().ToString();

                user.PasswordHash = hasher.HashPassword(
                    user,
                    inputUser.Password);

                user.Role = _context.Roles
                    .FirstOrDefault(r => r.Id == inputUser.RoleId)!;

                // Mantiene il comportamento originale.
                // Il codice originale non salva l'utente nel database.
                // Login automatico dopo registrazione.

                await Task.CompletedTask;

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Register));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Register),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // LOGIN GET
        // =========================================================

        [HttpGet]
        public IActionResult Login()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Login));

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Login));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Login),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // LOGIN POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginRequest request,
            string returnUrl = null)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Login));

            try
            {
                if (!string.IsNullOrWhiteSpace(request.LoginTicket))
                {
                    return await LoginWithTicketAsync(
                        request,
                        returnUrl);
                }

                return await LoginWithCredentialsAsync(
                    request,
                    returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Login));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Login),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // LOGIN WITH TICKET
        // =========================================================

        private async Task<IActionResult> LoginWithTicketAsync(
            LoginRequest request,
            string returnUrl)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(LoginWithTicketAsync));

            try
            {
                var pendingLogin = await _context.PendingLogins
                    .FirstOrDefaultAsync(
                        p => p.Ticket == request.LoginTicket);

                if (pendingLogin == null ||
                    pendingLogin.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ticket assente o scaduto",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

                    if (pendingLogin != null)
                    {
                        _context.PendingLogins.Remove(pendingLogin);
                        await _context.SaveChangesAsync();
                    }

                    ViewBag.Error =
                        "Sessione di login scaduta. Accedi nuovamente.";

                    return View("Login", new LoginRequest());
                }

                var allowedRoleIds = pendingLogin.AllowedRoleIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (request.RoleId == null ||
                    !allowedRoleIds.Contains(request.RoleId.Value))
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ruolo mancante o non autorizzato",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

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
                    .FirstOrDefaultAsync(
                        u => u.Id == pendingLogin.UserId);

                if (user == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: utente non trovato",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

                    _context.PendingLogins.Remove(pendingLogin);
                    await _context.SaveChangesAsync();

                    ViewBag.Error =
                        "Utente non trovato. Accedi nuovamente.";

                    return View("Login", new LoginRequest());
                }

                var selectedRole = await _context.Roles
                    .FirstOrDefaultAsync(
                        r => r.Id == request.RoleId.Value);

                if (selectedRole == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ruolo non trovato",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

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
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: utente senza ruoli",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

                    _context.PendingLogins.Remove(pendingLogin);
                    await _context.SaveChangesAsync();

                    ViewBag.Error = "Utente senza ruoli assegnati.";

                    return View("Login", new LoginRequest());
                }

                var originalRole = GetOriginalRole(originalRoles);

                if (originalRole == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ruolo originale non determinabile",
                        ControllerName,
                        nameof(LoginWithTicketAsync));

                    ViewBag.Error =
                        "Impossibile determinare il ruolo originale.";

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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(LoginWithTicketAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(LoginWithTicketAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // LOGIN WITH CREDENTIALS
        // =========================================================

        private async Task<IActionResult> LoginWithCredentialsAsync(
            LoginRequest request,
            string returnUrl)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(LoginWithCredentialsAsync));

            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: credenziali mancanti",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error =
                        "Username e password obbligatori.";

                    return View("Login", request);
                }

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(
                        u => u.UserName == request.UserName);

                if (user == null ||
                    string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: credenziali non valide",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

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
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: credenziali non valide",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error = "Credenziali errate.";

                    return View("Login", request);
                }

                var originalRoles = user.UserRoles
                    .Select(ur => ur.Role)
                    .ToList();

                if (originalRoles.Count == 0)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: utente senza ruoli",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error = "Utente senza ruoli assegnati.";

                    return View("Login", request);
                }

                var availableRoles = await GetAvailableRolesAsync(
                    originalRoles);

                if (availableRoles.Count == 0)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: nessun ruolo disponibile",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error =
                        "Non sono disponibili ruoli per questo utente.";

                    return View("Login", request);
                }

                if (availableRoles.Count > 1 &&
                    request.RoleId == null)
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
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ruolo selezionato non valido",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error = "Ruolo selezionato non valido.";
                    ViewBag.Roles = availableRoles;

                    return View("Login", request);
                }

                var originalRole = GetOriginalRole(originalRoles);

                if (originalRole == null)
                {
                    _logger.LogWarning(
                        "WARNING | Controller: {Controller} | Method: {Method} | Motivo: ruolo originale non determinabile",
                        ControllerName,
                        nameof(LoginWithCredentialsAsync));

                    ViewBag.Error =
                        "Impossibile determinare il ruolo originale.";

                    return View("Login", request);
                }

                return await CompleteLoginAsync(
                    user.Id,
                    originalRole,
                    selectedRole.Name,
                    returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(LoginWithCredentialsAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(LoginWithCredentialsAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // GET AVAILABLE ROLES
        // =========================================================

        private async Task<List<Role>> GetAvailableRolesAsync(
            List<Role> originalRoles)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(GetAvailableRolesAsync));

            try
            {
                var isAdmin = originalRoles.Any(
                    r => r.Name == "Admin");

                var isEmployee = originalRoles.Any(
                    r => r.Name == "Employee");

                if (isAdmin)
                {
                    return await _context.Roles
                        .OrderBy(r => r.Name)
                        .ToListAsync();
                }

                if (isEmployee)
                {
                    return await _context.Roles
                        .Where(r =>
                            r.Name == "Employee" ||
                            r.Name == "Customer" ||
                            r.Name == "Company")
                        .OrderBy(r => r.Name)
                        .ToListAsync();
                }

                return originalRoles;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(GetAvailableRolesAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(GetAvailableRolesAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // GET ORIGINAL ROLE
        // =========================================================

        private string GetOriginalRole(List<Role> roles)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(GetOriginalRole));

            try
            {
                return roles.FirstOrDefault(
                           r => r.Name == "Admin")?.Name
                       ?? roles.FirstOrDefault(
                           r => r.Name == "Employee")?.Name
                       ?? roles.FirstOrDefault()?.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(GetOriginalRole));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(GetOriginalRole),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // CREATE PENDING LOGIN
        // =========================================================

        private async Task<IActionResult> CreatePendingLoginAsync(
            string userId,
            List<Role> availableRoles,
            LoginRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(CreatePendingLoginAsync));

            try
            {
                var previousTickets = await _context.PendingLogins
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                _context.PendingLogins.RemoveRange(previousTickets);

                var ticket = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32));

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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(CreatePendingLoginAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(CreatePendingLoginAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // COMPLETE LOGIN
        // =========================================================

        private async Task<IActionResult> CompleteLoginAsync(
            string userId,
            string originalRole,
            string selectedRole,
            string returnUrl)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(CompleteLoginAsync));

            try
            {
                var token = _jwtService.GenerateToken(
                    userId,
                    originalRole,
                    selectedRole);

                Response.Cookies.Append(
                    "jwt",
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                _logger.LogInformation(
                    "Autenticazione completata | Controller: {Controller} | Method: {Method} | Role: {Role}",
                    ControllerName,
                    nameof(CompleteLoginAsync),
                    selectedRole);

                // Unisce il carrello ospite al carrello dell'utente.
                await MergeGuestCartAfterLoginAsync(userId);

                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(CompleteLoginAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(CompleteLoginAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        public IActionResult Logout()
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(Logout));

            try
            {
                Response.Cookies.Delete("jwt");

                _logger.LogInformation(
                    "Logout completato | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Logout));

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(Logout));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(Logout),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        // =========================================================
        // MERGE GUEST CART
        // =========================================================

        private async Task MergeGuestCartAfterLoginAsync(string userId)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "START | Controller: {Controller} | Method: {Method}",
                ControllerName,
                nameof(MergeGuestCartAfterLoginAsync));

            try
            {
                // Recupera il token del carrello ospite dal cookie.
                var guestCartToken = _cartHelper.GetGuestCartToken();

                // Se non esiste un carrello ospite, non c'è nulla da unire.
                if (string.IsNullOrWhiteSpace(guestCartToken))
                {
                    _logger.LogInformation(
                        "Nessun carrello ospite da unire | Controller: {Controller} | Method: {Method}",
                        ControllerName,
                        nameof(MergeGuestCartAfterLoginAsync));

                    return;
                }

                // Unisce il carrello ospite al carrello dell'utente autenticato.
                await _shoppingCartService.MergeGuestCartAsync(
                    guestCartToken,
                    userId);

                // Cancella il cookie soltanto dopo il merge riuscito.
                _cartHelper.ClearGuestCartToken();

                _logger.LogInformation(
                    "Carrello ospite unito correttamente | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(MergeGuestCartAfterLoginAsync));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR | Controller: {Controller} | Method: {Method}",
                    ControllerName,
                    nameof(MergeGuestCartAfterLoginAsync));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "END | Controller: {Controller} | Method: {Method} | Duration: {Duration} ms",
                    ControllerName,
                    nameof(MergeGuestCartAfterLoginAsync),
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}