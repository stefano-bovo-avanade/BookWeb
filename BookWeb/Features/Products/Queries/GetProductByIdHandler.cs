using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using MediatR;

namespace BookWeb.Features.Products.Queries
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Product>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _productRepository.GetProduct(request.ProductId);
        }
    }
}