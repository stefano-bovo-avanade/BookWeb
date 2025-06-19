using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BookWeb.Utility.ShoppingCartHelper
{
    public class CartHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CookieName = "CartToken";

        public CartHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCartToken()
        {
            var context = _httpContextAccessor.HttpContext!;
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            }

            if (!context.Request.Cookies.TryGetValue(CookieName, out var token))
            {
                token = Guid.NewGuid().ToString();
                context.Response.Cookies.Append(CookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return token;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
