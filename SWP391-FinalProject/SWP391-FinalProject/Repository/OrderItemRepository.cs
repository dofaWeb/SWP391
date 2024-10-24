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
                         join pt in db.ProductItems on ot.ProductItemId equals pt.Id
                         join p in db.Products on pt.ProductId equals p.Id
                         where ot.OrderId == orderId
                         select new OrderItemModel()
                         {
                             OrderId = ot.OrderId,
                             Discount = ot.Discount,
                             Price = ot.Price,
                             Quantity = ot.Quantity,
                             ProductItemId = ot.ProductItemId,
                             Product = new ProductModel()
                             {
                                 Name = p.Name,
                                 Picture = p.Picture
                             },
                         };
            List<OrderItemModel> orderItemList = result.ToList();
            foreach (var item in orderItemList)
            {
                ProductRepository proRepo = new ProductRepository();
                item.Ram = proRepo.GetProductVariationOption(item.ProductItemId, "Ram");
                item.Storage = proRepo.GetProductVariationOption(item.ProductItemId, "Storage");
            }
            return orderItemList;
        }
    }
}
