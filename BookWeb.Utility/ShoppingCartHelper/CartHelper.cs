using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BookWeb.Utility.ShoppingCartHelper
{
    public class CartHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShoppingCartRepository _shoppingCartRepository;

        private const string CookieName = "CartToken";

        public CartHelper(
            IHttpContextAccessor httpContextAccessor,
            IShoppingCartRepository shoppingCartRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _shoppingCartRepository = shoppingCartRepository;
        }

        public string GetCartToken()
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException(
                    "HttpContext non disponibile.");

            if (context.User.Identity?.IsAuthenticated == true)
            {
                return context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException(
                        "ID utente autenticato non disponibile.");
            }

            if (!context.Request.Cookies.TryGetValue(
                    CookieName, out var token)
                || string.IsNullOrWhiteSpace(token))
            {
                token = Guid.NewGuid().ToString("N");

                context.Response.Cookies.Append(
                    CookieName,
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        IsEssential = true
                    });
            }

            return token;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?
                .User.Identity?.IsAuthenticated ?? false;
        }

        public async Task<ShoppingCart?> GetCurrentCartAsync()
        {
            var token = GetCartToken();

            return IsAuthenticated()
                ? await _shoppingCartRepository.GetCartByUserIdAsync(token)
                : await _shoppingCartRepository.GetCartByCartTokenAsync(token);
        }

        public string? GetGuestCartToken()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return null;

            return context.Request.Cookies.TryGetValue(
                CookieName, out var token)
                ? token
                : null;
        }

        public void ClearGuestCartToken()
        {
            var context = _httpContextAccessor.HttpContext;

            context?.Response.Cookies.Delete(
                CookieName,
                new CookieOptions
                {
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = true
                });
        }

    }
}