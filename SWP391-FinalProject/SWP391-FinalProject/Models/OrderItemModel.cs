namespace SWP391_FinalProject.Models
{
    public class OrderItemModel
    {
        public ProductModel Product { get; set; }
        
        public string OrderId { get; set; }

        public string ProductItemId { get; set; }

        public int Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Discount { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderModel Order  { get; set; }

        public string Ram {  get; set; }
        public string Storage { get; set; }
    }
}
