using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookWeb.Models.Models;
using BookWeb.Features.Products.Queries;

namespace BookWeb.Areas.Customer.Views.Home;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IEnumerable<VProduct> Products { get; private set; }
        = Enumerable.Empty<VProduct>();

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        Products = await _mediator.Send(new GetProductsQuery());
    }
}