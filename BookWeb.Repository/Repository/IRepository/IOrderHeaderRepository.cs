using BookWeb.Models;
using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IOrderHeaderRepository
    {
        public Task<OrderHeader> GetOrderHeader(int? Id);

        public Task<IEnumerable<OrderHeader>> GetOrderHeaders();

        public Task<int> InsertOrderHeader(OrderHeader OrderHeader);

        public Task<int> UpdateOrderHeader(OrderHeader OrderHeader);

        public Task<int> DeleteOrderHeader(int? id);
    }
}
