using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookWeb.Areas.Admin.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : BaseController
    {
        private readonly string _connectionString = "Server=.; Database=BookWeb; Trusted_Connection=True; TrustServerCertificate=True;";
        
        [HttpGet("excel")]
        public IActionResult ExportToExcel()
        {

            string[] tabelle = { "Categories", "Companies", "OrderDetails", "Orders", "Products", "Roles", "ShoppingCarts", "ShoppingCartItems", "Users" };

            using var workbook = new XLWorkbook();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            foreach (var tabella in tabelle)
            {
                var query = $"SELECT * FROM {tabella}";
                using var command = new SqlCommand(query, connection);
                using var adapter = new SqlDataAdapter(command);
                var dt = new DataTable();
                adapter.Fill(dt);
                workbook.Worksheets.Add(dt, tabella);
            }

            using var stream = new MemoryStream();

            string percorsoDestinazione = @"C:\Users\stefano.bovo\Desktop\TabelleEsportate.xlsx";
            workbook.SaveAs(percorsoDestinazione);
            return Ok("File salvato correttamente");


        }

    }
}
