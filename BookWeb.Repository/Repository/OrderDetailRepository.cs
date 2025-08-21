using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;

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
                $"OrderId, " +
                $"Order, " +
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
                $"OrderId, " +
                $"Order, " +
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

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsPerOrderId(int Id)
        {
            var query = $"SELECT " +
                $"Id, " +
                $"OrderId, " +
                $"ProductId, " +
                $"Count, " +
                $"Price " +
                $"FROM OrderDetails " +
                $"where OrderId = @Id; " +
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
                    $"(OrderId, " +
                    $"ProductId, " +
                    $"Count, " +
                    $"Price) " +
                    $"VALUES " +
                    $"(@OrderId, " +
                    $"@ProductId, " +
                    $"@Count, " +
                    $"@Price); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    { 
                        OrderId = OrderDetail.OrderId,
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
                    $"OrderId = @OrderId, " +
                    $"Order = @Order, " +
                    $"ProductId = @ProductId, " +
                    $"Product = @Product, " +
                    $"Count = @Count, " +
                    $"Price = @Price; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    {
                        OrderId = OrderDetail.OrderId,
                        Order = OrderDetail.Order,
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
