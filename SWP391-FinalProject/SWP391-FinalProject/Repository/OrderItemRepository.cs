using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class OrderItemRepository
    {
        DBContext db = new DBContext();
        public OrderItemRepository()
        {
            
        }

        public List<OrderItemModel> GetOrderItemByOrderId(string orderId)
        {
            var result = from ot in db.OrderItems
                         where ot.OrderId == orderId
                         select new OrderItemModel()
                         {
                             OrderId = ot.OrderId,
                             Discount = ot.Discount,
                             Price = ot.Price,
                             Quantity = ot.Quantity,
                             ProductItemId = ot.ProductItemId
                         };

            List<OrderItemModel> orderItemList = result.ToList();
            return orderItemList;
        }
    }
}
