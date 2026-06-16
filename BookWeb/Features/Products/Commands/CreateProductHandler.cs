using BookWeb.Models.Models;
using BookWeb.Service.Context;
using MediatR;

namespace BookWeb.Features.Products.Commands
{
    public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, int>
    {
        private readonly ApplicationDbContext _context;

        public CreateProductCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Title = request.Title,
                Description = request.Description,
                ISBN = request.ISBN,
                Author = request.Author,
                ListPrice = request.ListPrice,
                Price = request.Price,
                Price50 = request.Price50,
                Price100 = request.Price100,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
