using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace BookWeb.Repository.Repository
{
    public class OrderHeaderRepository : IOrderHeaderRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger _logger;
        public OrderHeaderRepository(DapperContext context, ILogger<OrderDetailRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderHeader> GetOrderHeader(int? Id)
        {
            var query = $"SELECT " +
                $"Id," +
                $"ApplicationUserId," +
                $"OrderDate," +
                $"ShippingDate," +
                $"OrderTotal," +
                $"OrderStatus," +
                $"PaymentStatus," +
                $"TrackingNumber," +
                $"Carrier," +
                $"PaymentDate," +
                $"PaymentDueDate," +
                $"PaymentIntentId," +
                $"PhoneNumber," +
                $"Name," +
                $"StreetAddress," +
                $"City," +
                $"State," +
                $"PostalCode, " +
                $"SessionId " +
                $"FROM [Bulky].[dbo].[OrderHeaders] " +
                $"WHERE Id = @Id;" +
                $"SELECT * from dbo.Users";

            using (var connection = _context.CreateConnection())
            {
                OrderHeader? orderHeader = null;
                List<User?> Users = null;
                using (var lists = await connection.QueryMultipleAsync(query, new { Id = Id }))
                {
                    orderHeader = lists.Read<OrderHeader>().FirstOrDefault();
                    Users = lists.Read<User>().ToList();
                }

                if (orderHeader != null)
                {
                    orderHeader.ApplicationUser = Users.Where(u => u.Id.ToString() == orderHeader.ApplicationUserId).FirstOrDefault();
                }
                return orderHeader;

            }
        }

        public async Task<IEnumerable<OrderHeader>> GetOrderHeaders()
        {
            string query = $"SELECT" +
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
                $"[PostalCode], " +
                $"[SessionId] " +
                $"FROM [Bulky].[dbo].[OrderHeaders]; " +
                $"SELECT * from dbo.Users";

            using (var connection = _context.CreateConnection())
            {
                List<OrderHeader>? orderHeaders = null;
                List<User>? Users = null;
                using (var lists = await connection.QueryMultipleAsync(query))
                {
                    orderHeaders = lists.Read<OrderHeader>().ToList();
                    Users = lists.Read<User>().ToList();
                }

                foreach (var orderHeader in orderHeaders)
                {
                    orderHeader.ApplicationUser = Users.Where(u => u.Id.ToString() == orderHeader.ApplicationUserId).FirstOrDefault();
                }
                return orderHeaders;

            }
        }

        public async Task<IEnumerable<OrderHeader>> GetOrderHeaderPerApplicationUserId(string applicationUserId)
        {
            string query = $"SELECT" +
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
                $"[PostalCode], " +
                $"[SessionId] " +
                $"FROM [Bulky].[dbo].[OrderHeaders] " +
                $"WHERE ApplicationUserId = @ApplicationUserId; " +
                $"SELECT * from dbo.Users";

            using (var connection = _context.CreateConnection())
            {
                List<OrderHeader>? orderHeaders = null;
                List<User>? Users = null;
                using (var lists = await connection.QueryMultipleAsync(query, new { ApplicationUserId = applicationUserId}))
                {
                    orderHeaders = lists.Read<OrderHeader>().ToList();
                    Users = lists.Read<User>().ToList();
                }

                foreach (var orderHeader in orderHeaders)
                {
                    orderHeader.ApplicationUser = Users.Where(u => u.Id == orderHeader.ApplicationUserId).FirstOrDefault();
                }
                return orderHeaders;

            }
        }

        public async Task<int> InsertOrderHeader(OrderHeader OrderHeader)
        {
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new
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
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<int>("[dbo].[InsertUpdateOrderHeader]", parameters, commandType: CommandType.StoredProcedure);
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

        public async Task<int> UpdateOrderHeader(OrderHeader OrderHeader)
        {
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new
            {
                Id = OrderHeader.Id,
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
                SessionId = OrderHeader.SessionId,
            });
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = connection.QueryAsync<int>("[dbo].[InsertUpdateOrderHeader]", parameters, commandType: CommandType.StoredProcedure);
                    var b = parameters.Get<int?>("@returnValue");
                    return result.Result.FirstOrDefault();
                }
            }
            catch (Exception ex) 
            {
                throw;
            }


        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader header = GetOrderHeader(id).Result;
            
            if (header != null)
            {
                header.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    header.PaymentStatus = paymentStatus;
                }
                var result = UpdateOrderHeader(header);
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            OrderHeader header = GetOrderHeader(id).Result;

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
                var result = UpdateOrderHeader(header);
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
