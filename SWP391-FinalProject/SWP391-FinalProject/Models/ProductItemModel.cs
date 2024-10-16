namespace SWP391_FinalProject.Models
{
    public class ProductItemModel
    {
        public string Id {  get; set; }
        
        public string ProductId { get; set; }

        public ProductModel Product { get; set; }

        public int Quantity { get; set; }
        
        public decimal ImportPrice { get; set; }

        public decimal? SellingPrice { get; set; }

        public decimal? Discount { get; set; }

        public decimal? PriceAfterDiscount { get; set; }

        public decimal? Profit { get; set; }
        public decimal? Saving { get; set; }

        public VariationOptionModel Variations { get; set; }

        public string Ram { get; set; }

        public string Storage { get; set; }
    }
}
