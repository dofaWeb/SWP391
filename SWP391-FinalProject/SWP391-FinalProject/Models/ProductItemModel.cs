namespace SWP391_FinalProject.Models
{
    public class ProductItemModel
    {
        public string Id {  get; set; }
        
        public string ProductId { get; set; }

        public ProductModel Product { get; set; }

        public int Quantity { get; set; }
        
        public double ImportPrice { get; set; }

        public double SellingPrice { get; set; }

        public double Discount { get; set; }

        public List<VariationOptionModel> Variations { get; set; }
    }
}
