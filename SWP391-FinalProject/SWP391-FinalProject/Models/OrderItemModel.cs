namespace SWP391_FinalProject.Models
{
    public class OrderItemModel
    {
        public string OrderId { get; set; }

        public string ProductItemId { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public double Discount { get; set; }

        public OrderModel Order  { get; set; }
    }
}
