using BookWeb.Models;
using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IOrderRepository
    {
        public Task<Order> GetOrder(int? Id);

        public Task<IEnumerable<Order>> GetOrders();

        public Task<IEnumerable<Order>> GetOrderPerUserId(string UserId);

        public Task<int> InsertOrder(Order Order);

        public Task<int> UpdateOrder(Order Order);

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);

        public Task<int> DeleteOrder(int? id);
    }
}
