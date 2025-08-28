using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Service.IService;

namespace BookWeb.Service.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
       
        public OrderDetailService(
            IOrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        
        public async Task<OrderDetail> GetOrderDetail(int? Id)
        {
            return await _orderDetailRepository.GetOrderDetail(Id);
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetails()
        {
            return await _orderDetailRepository.GetOrderDetails();
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsPerOrderId(int Id) 
        {
            return await _orderDetailRepository.GetOrderDetailsPerOrderId(Id);
        }

        public async Task<int> InsertOrderDetail(OrderDetail OrderDetail) 
        {
            return await _orderDetailRepository.InsertOrderDetail(OrderDetail);
        }

        public async Task<int> UpdateOrderDetail(OrderDetail OrderDetail) 
        {
            return await _orderDetailRepository.UpdateOrderDetail(OrderDetail);
        }

        public async Task<int> DeleteOrderDetail(int? id)
        {
            return await _orderDetailRepository.DeleteOrderDetail(id);
        }
    }
}
