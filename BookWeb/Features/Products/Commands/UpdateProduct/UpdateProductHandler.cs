using BookWeb.Service.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductHandler
        : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public UpdateProductHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
            {
                return false;
            }

            product.Title = request.Title;
            product.Description = request.Description;
            product.ISBN = request.ISBN;
            product.Author = request.Author;
            product.ListPrice = request.ListPrice;
            product.Price = request.Price;
            product.Price50 = request.Price50;
            product.Price100 = request.Price100;
            product.CategoryId = request.CategoryId;
            product.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}