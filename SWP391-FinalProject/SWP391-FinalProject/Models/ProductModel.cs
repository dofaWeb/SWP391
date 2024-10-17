using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Models
{
    public class ProductModel
    {
        public ProductModel Products {get;set; }
        public List<ProductItemModel> ProductItems { get; set; }
        public ProductItemModel ProductItem { get; set; }
      
        public List<CommentModel> Comments { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int Quantity { get; set; }

        public string ProductState { get; set; }
        public int StateId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? DiscountedPrice { get; set; }
        
        public decimal? Saving { get; set;}

    }
}
