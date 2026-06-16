using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using MediatR;

namespace BookWeb.Features.Products.Queries
{
    public class GetProductsHandler : IRequestHandler<GetProductsQuery, IEnumerable<VProduct>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<VProduct>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            return await _productRepository.GetProducts();
        }
    }
}