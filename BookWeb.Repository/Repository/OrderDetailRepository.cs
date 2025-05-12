using BookWeb.Models.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;
using System.ComponentModel;

namespace BookWeb.Repository.Repository
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly DapperContext _context;
        public OrderDetailRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<OrderDetail> GetOrderDetail(int? Id)
        {
            var query = $"SELECT " +
                $"Id, " +
                $"OrderHeaderId, " +
                $"OrderHeader, " +
                $"ProductId, " +
                $"Product, " +
                $"Count, " +
                $"Price " +
                $"FROM OrderDetails where Id = @Id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<OrderDetail>(query, new { @Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetails()
        {
            var query = $"SELECT " +
                $"Id, " +
                $"OrderHeaderId, " +
                $"OrderHeader, " +
                $"ProductId, " +
                $"Product, " +
                $"Count, " +
                $"Price " +
                $"FROM OrderDetails";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<OrderDetail>(query);
                return result.ToList();
            }
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsPerOrderHeaderId(int Id)
        {
            var query = $"SELECT " +
                $"Id, " +
                $"OrderHeaderId, " +
                $"ProductId, " +
                $"Count, " +
                $"Price " +
                $"FROM OrderDetails " +
                $"where OrderHeaderId = @Id; " +
                $"SELECT * FROM Products";

            using (var connection = _context.CreateConnection())
            {
                List<OrderDetail> orderDetails = null;
                List<Product> products = null;

                using (var lists = await connection.QueryMultipleAsync(query, new { Id = Id }))
                {
                    orderDetails = lists.Read<OrderDetail>().ToList();
                    products = lists.Read<Product>().ToList();
                }
                
                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.Product = products.Where(p => p.Id == orderDetail.ProductId).FirstOrDefault();
                }
                return orderDetails;
            }
        }

        public async Task<int> InsertOrderDetail(OrderDetail OrderDetail)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"INSERT INTO OrderDetails " +
                    $"(OrderHeaderId, " +
                    $"ProductId, " +
                    $"Count, " +
                    $"Price) " +
                    $"VALUES " +
                    $"(@OrderHeaderId, " +
                    $"@ProductId, " +
                    $"@Count, " +
                    $"@Price); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    { 
                        OrderHeaderId = OrderDetail.OrderHeaderId,
                        ProductId = OrderDetail.ProductId,
                        Count = OrderDetail.Count,
                        Price = OrderDetail.Price,
                    });
                
                return result.FirstOrDefault();
                
            }
        }

        public async Task<int> UpdateOrderDetail(OrderDetail OrderDetail)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE OrderDetails SET " +
                    $"OrderHeaderId = @OrderHeaderId, " +
                    $"OrderHeader = @OrderHeader, " +
                    $"ProductId = @ProductId, " +
                    $"Product = @Product, " +
                    $"Count = @Count, " +
                    $"Price = @Price; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    {
                        OrderHeaderId = OrderDetail.OrderHeaderId,
                        OrderHeader = OrderDetail.OrderHeader,
                        ProductId = OrderDetail.ProductId,
                        Product = OrderDetail.Product,
                        Count = OrderDetail.Count,
                        Price = OrderDetail.Price,
                        
                    });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteOrderDetail(int? id)
        {
            string query = "Delete from dbo.OrderDetails where id = @id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new { id = id });
                return result;
            }
        }
    }
}
