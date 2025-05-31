using BookWeb.Models;
using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IOrderHeaderRepository
    {
        public Task<OrderHeader> GetOrderHeader(int? Id);

        public Task<IEnumerable<OrderHeader>> GetOrderHeaders();

        public Task<IEnumerable<OrderHeader>> GetOrderHeaderPerUserId(string UserId);

        public Task<int> InsertOrderHeader(OrderHeader OrderHeader);

        public Task<int> UpdateOrderHeader(OrderHeader OrderHeader);

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);

        public Task<int> DeleteOrderHeader(int? id);
    }
}
