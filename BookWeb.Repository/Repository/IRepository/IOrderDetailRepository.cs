using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface IOrderDetailRepository
    {
        public Task<OrderDetail> GetOrderDetail(int? Id);

        public Task<IEnumerable<OrderDetail>> GetOrderDetails();

        public Task<IEnumerable<OrderDetail>> GetOrderDetailsPerOrderId(int Id);

        public Task<int> InsertOrderDetail(OrderDetail OrderDetail);

        public Task<int> UpdateOrderDetail(OrderDetail OrderDetail);

        public Task<int> DeleteOrderDetail(int? id);
    }
}
