using BookWeb.Models.Models;

namespace BookWeb.Models.ViewModels
{
    public class OrderVM
    {
        public Order Order { get; set; }

        public IEnumerable<OrderDetail> OrderDetailList { get; set; }
    }
}
