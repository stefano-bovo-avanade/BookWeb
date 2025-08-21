using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.EntityFrameworkCore;


namespace BookWeb.Repository.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _applicationDbContext;

        public OrderRepository(ApplicationDbContext applicationDbContext,
            DapperContext context, ILogger<OrderDetailRepository> logger)
        {
            _context = context;
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Order> GetOrder(int? Id)
        {
            var order = _applicationDbContext.Orders.Include(o => o.User).Where(o => o.Id == Id).FirstOrDefault();
            return order;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            var orders = _applicationDbContext.Orders.Include(o => o.User);
            return orders;
        }

        public async Task<IEnumerable<Order>> GetOrderPerUserId(string UserId)
        {
            string query = $"SELECT" +
                $"[Id]," +
                $"[UserId]," +
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
                $"[PostalCode], " +
                $"[SessionId] " +
                $"FROM [BookWeb].[dbo].[Orders] " +
                $"WHERE UserId = @UserId; " +
                $"SELECT * from dbo.Users";

            using (var connection = _context.CreateConnection())
            {
                List<Order>? orders = null;
                List<User>? Users = null;
                using (var lists = await connection.QueryMultipleAsync(query, new { UserId = UserId}))
                {
                    orders = lists.Read<Order>().ToList();
                    Users = lists.Read<User>().ToList();
                }

                foreach (var order in orders)
                {
                    order.User = Users.Where(u => u.Id == order.UserId).FirstOrDefault()!;
                }
                return orders;

            }
        }

        public async Task<int> InsertOrder(Order Order)
        {
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new
            {
                UserId = Order.UserId,
                OrderDate = Order.OrderDate,
                ShippingDate = Order.ShippingDate,
                OrderTotal = Order.OrderTotal,
                OrderStatus = Order.OrderStatus,
                PaymentStatus = Order.PaymentStatus,
                TrackingNumber = Order.TrackingNumber,
                Carrier = Order.Carrier,
                PaymentDate = Order.PaymentDate,
                PaymentDueDate = Order.PaymentDueDate,
                PaymentIntentId = Order.PaymentIntentId,
                PhoneNumber = Order.PhoneNumber,
                Name = Order.Name,
                StreetAddress = Order.StreetAddress,
                City = Order.City,
                State = Order.State,
                PostalCode = Order.PostalCode,
            });
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<int>("[dbo].[InsertUpdateOrder]", parameters, commandType: CommandType.StoredProcedure);
                    int b = parameters.Get<int>("@returnValue");
                    return b;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Errore " + ex.Message);
                throw;
            }
            
        }

        public async Task<int> UpdateOrder(Order Order)
        {
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new
            {
                Id = Order.Id,
                UserId = Order.UserId,
                OrderDate = Order.OrderDate,
                ShippingDate = Order.ShippingDate,
                OrderTotal = Order.OrderTotal,
                OrderStatus = Order.OrderStatus,
                PaymentStatus = Order.PaymentStatus,
                TrackingNumber = Order.TrackingNumber,
                Carrier = Order.Carrier,
                PaymentDate = Order.PaymentDate,
                PaymentDueDate = Order.PaymentDueDate,
                PaymentIntentId = Order.PaymentIntentId,
                PhoneNumber = Order.PhoneNumber,
                Name = Order.Name,
                StreetAddress = Order.StreetAddress,
                City = Order.City,
                State = Order.State,
                PostalCode = Order.PostalCode,
                SessionId = Order.SessionId,
            });
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<int>("[dbo].[InsertUpdateOrder]", parameters, commandType: CommandType.StoredProcedure);
                    var b = parameters.Get<int?>("@returnValue");
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex) 
            {
                throw;
            }


        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            Order header = GetOrder(id).Result;
            
            if (header != null)
            {
                header.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    header.PaymentStatus = paymentStatus;
                }
                var result = UpdateOrder(header);
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            Order header = GetOrder(id).Result;

            if (header != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    header.SessionId = sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    header.PaymentIntentId = paymentIntentId;
                    header.PaymentDate = DateTime.Now;
                }
                var result = UpdateOrder(header);
            }
        }

        public async Task<int> DeleteOrder(int? id)
        {
            string query = "Delete from dbo.Orders where id = @id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new { id = id });
                return result;
            }
        }
    }
}
