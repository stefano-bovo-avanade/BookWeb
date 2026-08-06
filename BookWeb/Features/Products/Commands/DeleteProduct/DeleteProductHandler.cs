using BookWeb.Service.Context;
using MediatR;

namespace BookWeb.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly ApplicationDbContext _context;

        public DeleteProductHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(request.Id);

            if (product == null)
                throw new Exception("Prodotto non trovato");

            _context.Products.Remove(product);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}