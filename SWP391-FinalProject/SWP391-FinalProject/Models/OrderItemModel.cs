﻿namespace SWP391_FinalProject.Models
{
    public class OrderItemModel
    {
        public string OrderId { get; set; }

        public string ProductItemId { get; set; }

        public int Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Discount { get; set; }

        public OrderModel Order  { get; set; }
    }
}
