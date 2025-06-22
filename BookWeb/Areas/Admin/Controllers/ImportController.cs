using BookWeb.Models.Models;
using BookWeb.Service.Context;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File non valido.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);

            // Categories
            foreach (var row in workbook.Worksheet("Categories").RangeUsed().RowsUsed().Skip(1))
            {
                _context.Categories.Add(new Category
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    Name = row.Cell(2).GetString(),
                    DisplayOrder = int.Parse(row.Cell(3).GetString())
                });
            }

            // Companies
            foreach (var row in workbook.Worksheet("Companies").RangeUsed().RowsUsed().Skip(1))
            {
                _context.Companies.Add(new Company
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    Name = row.Cell(2).GetString(),
                    StreetAddress = row.Cell(3).GetString(),
                    City = row.Cell(4).GetString(),
                    State = row.Cell(5).GetString(),
                    PostalCode = row.Cell(6).GetString(),
                    PhoneNumber = row.Cell(7).GetString()
                });
            }

            // Products
            foreach (var row in workbook.Worksheet("Products").RangeUsed().RowsUsed().Skip(1))
            {
                _context.Products.Add(new Product
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    Title = row.Cell(2).GetString(),
                    Author = row.Cell(3).GetString(),
                    Description = row.Cell(4).GetString(),
                    ISBN = row.Cell(5).GetString(),
                    ListPrice = double.Parse(row.Cell(6).GetString()),
                    Price = double.Parse(row.Cell(7).GetString()),
                    Price50 = double.Parse(row.Cell(8).GetString()),
                    Price100 = double.Parse(row.Cell(9).GetString()),
                    CategoryId = int.Parse(row.Cell(10).GetString()),
                    ImageUrl = row.Cell(11).GetString()
                });
            }

            // Roles
            foreach (var row in workbook.Worksheet("Roles").RangeUsed().RowsUsed().Skip(1))
            {
                _context.Roles.Add(new Role
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    Name = row.Cell(2).GetString()
                });
            }

            // ShoppingCarts
            foreach (var row in workbook.Worksheet("ShoppingCarts").RangeUsed().RowsUsed().Skip(1))
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    CartTotal = int.Parse(row.Cell(2).GetString()),
                    UserId = row.Cell(3).GetString(),
                    CartToken = row.Cell(4).GetString(),
                });
            }

            // ShoppingCartItems
            foreach (var row in workbook.Worksheet("ShoppingCartItems").RangeUsed().RowsUsed().Skip(1))
            {
                _context.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    ShoppingCartId = int.Parse(row.Cell(2).GetString()),
                    CartToken = row.Cell(3).GetString(),
                    ProductId = int.Parse(row.Cell(4).GetString()),
                    Count = int.Parse(row.Cell(5).GetString()),
                    Price = int.Parse(row.Cell(6).GetString())
                });
            }

            // Users
            foreach (var row in workbook.Worksheet("Users").RangeUsed().RowsUsed().Skip(1))
            {
                _context.Users.Add(new User
                {
                    Id = row.Cell(1).GetString(),
                    UserName = row.Cell(2).GetString(),
                    Email = row.Cell(3).GetString(),
                    PasswordHash = row.Cell(4).GetString(),
                    PhoneNumber = row.Cell(5).GetString(),
                    City = row.Cell(6).GetString(),
                    Name = row.Cell(7).GetString(),
                    PostalCode = row.Cell(8).GetString(),
                    State = row.Cell(9).GetString(),
                    StreetAddress = row.Cell(10).GetString(),
                    CompanyId = int.Parse(row.Cell(11).GetString()),
                    RoleId = int.Parse(row.Cell(12).GetString())
                });
            }

            // OrderHeaders
            foreach (var row in workbook.Worksheet("OrderHeaders").RangeUsed().RowsUsed().Skip(1))
            {
                _context.OrderHeaders.Add(new OrderHeader
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    UserId = row.Cell(2).GetString(),
                    OrderDate = DateTime.Parse(row.Cell(3).GetString()),
                    ShippingDate = DateTime.Parse(row.Cell(4).GetString()),
                    OrderTotal = double.Parse(row.Cell(5).GetString()),
                    OrderStatus = row.Cell(6).GetString(),
                    PaymentStatus = row.Cell(7).GetString(),
                    TrackingNumber = row.Cell(8).GetString(),
                    Carrier = row.Cell(9).GetString(),
                    PaymentDate = DateTime.TryParse(row.Cell(10).GetString(), out var pd) ? pd : null,
                    PaymentDueDate = DateTime.TryParse(row.Cell(11).GetString(), out var pdd) ? pdd : null,
                    PaymentIntentId = row.Cell(12).GetString(),
                    PhoneNumber = row.Cell(13).GetString(),
                    Name = row.Cell(14).GetString(),
                    StreetAddress = row.Cell(15).GetString(),
                    City = row.Cell(16).GetString(),
                    State = row.Cell(17).GetString(),
                    PostalCode = row.Cell(18).GetString(),
                    SessionId = row.Cell(19).GetString()
                });
            }

            // OrderDetails
            foreach (var row in workbook.Worksheet("OrderDetails").RangeUsed().RowsUsed().Skip(1))
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    Id = int.Parse(row.Cell(1).GetString()),
                    OrderHeaderId = int.Parse(row.Cell(2).GetString()),
                    ProductId = int.Parse(row.Cell(3).GetString()),
                    Count = int.Parse(row.Cell(4).GetString()),
                    Price = double.Parse(row.Cell(5).GetString())
                });
            }

            await _context.SaveChangesAsync();
            return Ok("Importazione completata.");
        }


    }

}
