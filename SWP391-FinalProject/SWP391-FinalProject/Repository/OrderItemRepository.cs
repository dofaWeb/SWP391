using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class OrderItemRepository
    {
       
        public OrderItemRepository()
        {

        }

        public List<OrderItemModel> GetOrderItemByOrderId(string orderId)
        {
            string query = "Select ot.order_id as OrderId, ot.discount as Discount, ot.price as Price, ot.quantity as Quantity, " +
                            "ot.product_item_id as ProductItemId, p.name as Name, p.picture as Picture " +
                            "From Order_Item ot " +
                            "Inner join Product_Item pt on ot.product_item_id = pt.id " +
                            "Inner join Product p on pt.product_id = p.id " +
                            "where ot.order_id = @OrderId";
            var parameters = new Dictionary<string, object>
            {
                { "@OrderId", orderId }
            };
            DataTable orderItemTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            List<OrderItemModel> orderItemList = new List<OrderItemModel>();
            foreach (DataRow row in orderItemTable.Rows)
            {
                orderItemList.Add(new OrderItemModel()
                {
                    OrderId = row["OrderId"].ToString(),
                    Discount = decimal.Parse(row["Discount"].ToString()),
                    Price = decimal.Parse(row["Price"].ToString()),
                    Quantity = int.Parse(row["Quantity"].ToString()),
                    ProductItemId = row["ProductItemId"].ToString(),
                    Product = new ProductModel()
                    {
                        Name = row["Name"].ToString(),
                        Picture = row["Picture"].ToString()
                    }
                });
            }

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
