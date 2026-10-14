using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Service.Context;
using BookWeb.Service.Service.IService;
using BookWeb.Utility.Authentication;
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

        public AccountController(
            IConfiguration config,
            ApplicationDbContext context,
            IMapper mapper,
            IShoppingCartService shoppingCartService,
            JwtService jwtService)
        {
            _config = config;
            _context = context;
            _mapper = mapper;
            _shoppingCartService = shoppingCartService;
            _jwtService = jwtService;
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
            // =========================================================
            // FASE 2: verifica del ticket e selezione del ruolo
            // =========================================================

            if (!string.IsNullOrWhiteSpace(request.LoginTicket))
            {
                // 1. Recuperiamo il ticket dal database.
                var pendingLogin = await _context.PendingLogins
                    .FirstOrDefaultAsync(p => p.Ticket == request.LoginTicket);

                // 2. Verifichiamo che il ticket esista e non sia scaduto.
                if (pendingLogin == null ||
                    pendingLogin.ExpiresAt <= DateTime.UtcNow)
                {
                    if (pendingLogin != null)
                    {
                        _context.PendingLogins.Remove(pendingLogin);
                        await _context.SaveChangesAsync();
                    }

                    ViewBag.Error = "Sessione di login scaduta. Accedi nuovamente.";
                    return View(new LoginRequest());
                }

                // 3. Verifichiamo che sia stato selezionato un ruolo.
                if (request.RoleId == null)
                {
                    ViewBag.Error = "Seleziona un ruolo.";
                    ViewBag.Roles = await _context.Roles
                        .Where(r => pendingLogin.AllowedRoleIds
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Contains(r.Id.ToString()))
                        .OrderBy(r => r.Name)
                        .ToListAsync();

                    return View(request);
                }

                // 4. Recuperiamo gli ID dei ruoli consentiti dal ticket.
                var allowedRoleIds = pendingLogin.AllowedRoleIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                // 5. Verifichiamo che il ruolo selezionato sia consentito.
                if (!allowedRoleIds.Contains(request.RoleId.Value))
                {
                    ViewBag.Error = "Ruolo selezionato non valido.";
                    ViewBag.Roles = await _context.Roles
                        .Where(r => allowedRoleIds.Contains(r.Id))
                        .OrderBy(r => r.Name)
                        .ToListAsync();

                    return View(request);
                }

                // 6. Recuperiamo l'utente associato al ticket.
                var ticketUser = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == pendingLogin.UserId);

                if (ticketUser == null)
                {
                    _context.PendingLogins.Remove(pendingLogin);
                    await _context.SaveChangesAsync();

                    ViewBag.Error = "Utente non trovato. Accedi nuovamente.";
                    return View(new LoginRequest());
                }

                // 7. Recuperiamo il ruolo selezionato.
                var ticketSelectedRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == request.RoleId.Value);

                if (ticketSelectedRole == null)
                {
                    ViewBag.Error = "Ruolo non valido.";
                    ViewBag.Roles = await _context.Roles
                        .Where(r => allowedRoleIds.Contains(r.Id))
                        .OrderBy(r => r.Name)
                        .ToListAsync();

                    return View(request);
                }

                // 8. Recuperiamo i ruoli originali dell'utente.
                var ticketOriginalRoles = ticketUser.UserRoles
                    .Select(ur => ur.Role)
                    .ToList();

                if (ticketOriginalRoles.Count == 0)
                {
                    _context.PendingLogins.Remove(pendingLogin);
                    await _context.SaveChangesAsync();

                    ViewBag.Error = "Utente senza ruoli assegnati.";
                    return View(new LoginRequest());
                }

                // 9. Determiniamo il ruolo originale.
                var ticketOriginalRole =
                    ticketOriginalRoles.FirstOrDefault(r => r.Name == "Admin")?.Name
                    ?? ticketOriginalRoles.FirstOrDefault(r => r.Name == "Employee")?.Name
                    ?? ticketOriginalRoles.FirstOrDefault()?.Name;

                if (ticketOriginalRole == null)
                {
                    ViewBag.Error = "Impossibile determinare il ruolo originale.";
                    return View(new LoginRequest());
                }

                // 10. Eliminiamo il ticket utilizzato.
                _context.PendingLogins.Remove(pendingLogin);
                await _context.SaveChangesAsync();

                // 11. Generiamo il JWT definitivo.
                var ticketToken = _jwtService.GenerateToken(
                    ticketUser.Id,
                    ticketOriginalRole,
                    ticketSelectedRole.Name);

                // 12. Salviamo il JWT nel cookie.
                Response.Cookies.Append("jwt", ticketToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // 13. Reindirizziamo alla pagina richiesta, se locale.
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // =========================================================
            // FASE 1: verifica di username e password
            // =========================================================

            // 1. Verifichiamo che username e password siano presenti.
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                ViewBag.Error = "Username e password obbligatori.";
                return View(request);
            }

            // 2. Recuperiamo l'utente e i suoi ruoli.
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                ViewBag.Error = "Credenziali errate.";
                return View(request);
            }

            // 3. Verifichiamo la password.
            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Credenziali errate.";
                return View(request);
            }

            // 4. Recuperiamo i ruoli originali.
            var originalRoles = user.UserRoles
                .Select(ur => ur.Role)
                .ToList();

            if (originalRoles.Count == 0)
            {
                ViewBag.Error = "Utente senza ruoli assegnati.";
                return View(request);
            }

            var isAdmin = originalRoles.Any(r => r.Name == "Admin");
            var isEmployee = originalRoles.Any(r => r.Name == "Employee");

            // 5. Determiniamo i ruoli selezionabili.
            List<Role> availableRoles;

            if (isAdmin)
            {
                // Admin può operare in qualsiasi contesto.
                availableRoles = await _context.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            else if (isEmployee)
            {
                // Employee può operare come Employee, Customer o Company.
                availableRoles = await _context.Roles
                    .Where(r => r.Name == "Employee"
                             || r.Name == "Customer"
                             || r.Name == "Company")
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            else
            {
                // Gli altri utenti possono utilizzare soltanto i ruoli assegnati.
                availableRoles = originalRoles;
            }

            if (availableRoles.Count == 0)
            {
                ViewBag.Error = "Non sono disponibili ruoli per questo utente.";
                return View(request);
            }

            // 6. Se esiste un solo ruolo, lo selezioniamo automaticamente.
            if (availableRoles.Count == 1)
            {
                request.RoleId = availableRoles[0].Id;
            }

            // 7. Se occorre scegliere un ruolo, creiamo il ticket temporaneo.
            if (availableRoles.Count > 1 && request.RoleId == null)
            {
                // Eliminiamo eventuali ticket precedenti dell'utente.
                var previousTickets = await _context.PendingLogins
                    .Where(p => p.UserId == user.Id)
                    .ToListAsync();

                _context.PendingLogins.RemoveRange(previousTickets);

                // Generiamo un ticket casuale crittograficamente sicuro.
                var ticket = Convert.ToHexString(
                    System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

                // Salviamo il ticket e i ruoli consentiti.
                var pendingLogin = new PendingLogin
                {
                    Ticket = ticket,
                    UserId = user.Id,
                    AllowedRoleIds = string.Join(
                        ",",
                        availableRoles.Select(r => r.Id)),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                _context.PendingLogins.Add(pendingLogin);
                await _context.SaveChangesAsync();

                // Passiamo il ticket e i ruoli alla vista.
                request.LoginTicket = ticket;

                System.Diagnostics.Debug.WriteLine(
                    $"Ticket generato: {ticket}");

                System.Diagnostics.Debug.WriteLine(
                    $"Ticket nel modello: {request.LoginTicket}");

                ViewBag.Roles = availableRoles;

                return View(request);

                request.Password = string.Empty;

                ViewBag.Roles = availableRoles;

                return View(request);
            }

            // 8. Verifichiamo il ruolo selezionato.
            var selectedRole = availableRoles
                .FirstOrDefault(r => r.Id == request.RoleId);

            if (selectedRole == null)
            {
                ViewBag.Error = "Ruolo selezionato non valido.";
                ViewBag.Roles = availableRoles;
                return View(request);
            }

            // 9. Determiniamo il ruolo originale.
            var originalRole =
                originalRoles.FirstOrDefault(r => r.Name == "Admin")?.Name
                ?? originalRoles.FirstOrDefault(r => r.Name == "Employee")?.Name
                ?? originalRoles.FirstOrDefault()?.Name;

            if (originalRole == null)
            {
                ViewBag.Error = "Impossibile determinare il ruolo originale.";
                return View(request);
            }

            // 10. Generiamo il JWT definitivo.
            var token = _jwtService.GenerateToken(
                user.Id,
                originalRole,
                selectedRole.Name);

            // 11. Salviamo il JWT nel cookie.
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // 12. Reindirizziamo alla pagina richiesta, se locale.
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
    }
}
