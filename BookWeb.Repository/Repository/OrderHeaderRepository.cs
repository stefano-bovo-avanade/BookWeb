using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;

namespace BookWeb.Repository.Repository
{
    public class OrderHeaderRepository : IOrderHeaderRepository
    {
        private readonly DapperContext _context;
        public OrderHeaderRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<OrderHeader> GetOrderHeader(int? Id)
        {
            var query = $"SELECT " +
                $"[Id]," +
                $"[ApplicationUserId]," +
                $"[OrderDate]," +
                $"[ShippingDate]," +
                $"[OrderTotal]," +
                $"[OrderStatus]," +
                $"[PaymentStatus]," +
                $"[TrackingNumber]," +
                $"[Carrier]," +
                $"[PaymentDate]," +
                $"[PaymentDueDate]," +
                $"[PaymentIntentId]," +
                $"[PhoneNumber]," +
                $"[Name]," +
                $"[StreetAddress]," +
                $"[City]," +
                $"[State]," +
                $"[PostalCode] " +
                $"FROM [Bulky].[dbo].[OrderHeaders] " +
                $"WHERE Id = @Id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<OrderHeader>(query, new { @Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<OrderHeader>> GetOrderHeaders()
        {
            var query = $"SELECT" +
                $"[Id]," +
                $"[ApplicationUserId]," +
                $"[OrderDate]," +
                $"[ShippingDate]," +
                $"[OrderTotal]," +
                $"[OrderStatus]," +
                $"[PaymentStatus]," +
                $"[TrackingNumber]," +
                $"[Carrier]," +
                $"[PaymentDate]," +
                $"[PaymentDueDate]," +
                $"[PaymentIntentId]," +
                $"[PhoneNumber]," +
                $"[Name]," +
                $"[StreetAddress]," +
                $"[City]," +
                $"[State]," +
                $"[PostalCode] " +
                $"FROM [Bulky].[dbo].[OrderHeaders]";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<OrderHeader>(query);
                return result.ToList();
            }
        }

        public async Task<int> InsertOrderHeader(OrderHeader OrderHeader)
        {
            var sql = $"INSERT INTO OrderHeaders " +
                $"([ApplicationUserId], " +
                $"[OrderDate], " +
                $"[ShippingDate], " +
                $"[OrderTotal], " +
                $"[OrderStatus], " +
                $"[PaymentStatus], " +
                $"[TrackingNumber], " +
                $"[Carrier], " +
                $"[PaymentDate], " +
                $"[PaymentDueDate], " +
                $"[PaymentIntentId], " +
                $"[PhoneNumber], " +
                $"[Name], " +
                $"[StreetAddress], " +
                $"[City], " +
                $"[State], " +
                $"[PostalCode]) " +
                $"VALUES " +
                $"(@ApplicationUserId, " +
                $"@[OrderDate], " +
                $"@[ShippingDate], " +
                $"@[OrderTotal], " +
                $"@[OrderStatus], " +
                $"[PaymentStatus], " +
                $"@[TrackingNumber], " +
                $"@[Carrier], " +
                $"@[PaymentDate], " +
                $"@[PaymentDueDate], " +
                $"@[PaymentIntentId], " +
                $"@[PhoneNumber], " +
                $"@[Name], " +
                $"@[StreetAddress], " +
                $"@[City], " +
                $"@[State], " +
                $"@[PostalCode]); " +
                $"SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    { 
                        ApplicationUserId = OrderHeader.ApplicationUserId,
                        OrderDate = OrderHeader.OrderDate,
                        ShippingDate = OrderHeader.ShippingDate,
                        OrderTotal = OrderHeader.OrderTotal,
                        OrderStatus = OrderHeader.OrderStatus,
                        PaymentStatus = OrderHeader.PaymentStatus,
                        TrackingNumber = OrderHeader.TrackingNumber,
                        Carrier = OrderHeader.Carrier,
                        PaymentDate = OrderHeader.PaymentDate,
                        PaymentDueDate = OrderHeader.PaymentDueDate,
                        PaymentIntentId = OrderHeader.PaymentIntentId,
                        PhoneNumber = OrderHeader.PhoneNumber,
                        Name = OrderHeader.Name,
                        StreetAddress = OrderHeader.StreetAddress,
                        City = OrderHeader.City,
                        State = OrderHeader.State,
                        PostalCode = OrderHeader.PostalCode,

                    });
                
                return result.FirstOrDefault();
                
            }
        }

        public async Task<int> UpdateOrderHeader(OrderHeader OrderHeader)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE OrderHeaders SET " +
                    $"[ApplicationUserId] = @[ApplicationUserId], " +
                    $"[OrderDate] = @[OrderDate], " +
                    $"[ShippingDate]= @[ShippingDate], " +
                    $"[OrderTotal]= @[OrderTotal], " +
                    $"[OrderStatus]= @[OrderStatus], " +
                    $"[PaymentStatus]= @[PaymentStatus], " +
                    $"[TrackingNumber]= @,[TrackingNumber], " +
                    $"[Carrier] = @[Carrier], " +
                    $"[PaymentDate] = @[PaymentDate], " +
                    $"[PaymentDueDate] = @[PaymentDueDate], " +
                    $"[PaymentIntentId] = @[PaymentIntentId], " +
                    $"[PhoneNumber] = @[PhoneNumber], " +
                    $"[Name] = @[Name], " +
                    $"[StreetAddress] = @[StreetAddress], " +
                    $"[City] = @[City], " +
                    $"[State] = @[State];" +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                    new 
                    {
                        ApplicationUserId = OrderHeader.ApplicationUserId,
                        OrderDate = OrderHeader.OrderDate,
                        ShippingDate = OrderHeader.ShippingDate,
                        OrderTotal = OrderHeader.OrderTotal,
                        OrderStatus = OrderHeader.OrderStatus,
                        PaymentStatus = OrderHeader.PaymentStatus,
                        TrackingNumber = OrderHeader.TrackingNumber,
                        Carrier = OrderHeader.Carrier,
                        PaymentDate = OrderHeader.PaymentDate,
                        PaymentDueDate = OrderHeader.PaymentDueDate,
                        PaymentIntentId = OrderHeader.PaymentIntentId,
                        PhoneNumber = OrderHeader.PhoneNumber,
                        Name = OrderHeader.Name,
                        StreetAddress = OrderHeader.StreetAddress,
                        City = OrderHeader.City,
                        State = OrderHeader.State,
                        PostalCode = OrderHeader.PostalCode,
                    });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteOrderHeader(int? id)
        {
            string query = "Delete from dbo.OrderHeaders where id = @id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new { id = id });
                return result;
            }
        }
    }
}
