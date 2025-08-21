using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Repository.Repository
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart?> GetCartByUserIdAsync(string userId)
        {
            return await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<ShoppingCart?> GetCartByCartTokenAsync(string cartToken)
        {
            return await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(c => c.CartToken == cartToken);
        }

        public async Task AddCartAsync(ShoppingCart cart)
        {
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartAsync(ShoppingCart cart)
        {
            _context.ShoppingCarts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveShoppingCart(ShoppingCart shoppingCart)
        {
            _context.ShoppingCarts.Remove(shoppingCart);
            await _context.SaveChangesAsync(); 
        }

        public async Task RemoveShoppingCartItem(ShoppingCartItem shoppingCartItem)
        {
            _context.ShoppingCartItems.Remove(shoppingCartItem);
            await _context.SaveChangesAsync(); // Aggiunto
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
