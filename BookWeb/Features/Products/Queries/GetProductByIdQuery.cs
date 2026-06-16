using BookWeb.Models.Models;
using MediatR;

namespace BookWeb.Features.Products.Queries
{
    public class GetProductByIdQuery : IRequest<Product>
    {
        public int ProductId { get; }

        public GetProductByIdQuery(int productId)
        {
            ProductId = productId;
        }
    }
}