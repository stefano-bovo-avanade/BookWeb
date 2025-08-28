using BookWeb.Models.Models;

namespace BookWeb.Service.Service.IService
{
    public interface IOrderService
    {
        public Task<Order> GetOrderAsync(int userId);
        public Task<IEnumerable<Order>> GetOrderPerUserIdAsync(string UserId);
        public Task<IEnumerable<Order>> GetOrdersAsync();
        public Task<int> CreateOrderAsync(string userId, Order order);
        public Task<int> UpdateOrder(Order Order);
        public Task<string?> CreateStripeSessionAsync(int orderId);
        public Task UpdateOrderStatus(int id, string orderStatus, string? paymentStatus = null);
        public Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);

    }
}
