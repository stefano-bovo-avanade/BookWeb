using MediatR;
using BookWeb.Models.Models;

namespace BookWeb.Features.Products.Queries    
{

    public record GetProductsQuery() : IRequest<IEnumerable<VProduct>>;
}
