using BookWeb.Models;
using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IOrderRepository
    {
        public Task<Order> GetOrderAsync(int? Id);

        public Task<IEnumerable<Order>> GetOrdersAsync();

        public Task<IEnumerable<Order>> GetOrderPerUserIdAsync(string UserId);

        public Task<int> InsertOrder(Order Order);

        public Task<int> UpdateOrder(Order Order);

        public Task<int> DeleteOrder(int? id);
    }
}
