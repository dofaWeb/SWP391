using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.ViewComponents
{
    public class ProductCustomerViewComponent : ViewComponent
    {
        private readonly DBContext db;

        public ProductCustomerViewComponent(DBContext context) => db = context;

        public IViewComponentResult Invoke(string productType, string str = null)
        {
            Repository.Product proRepo = new Repository.Product(db);
            List<Models.ProductModel> products;

            // Decide which method to call based on the passed argument
            switch (productType)
            {
                case "TopSelling":
                    products = proRepo.GetAllProduct();
                    break;
                case "GetAllProduct":
                    products = proRepo.GetAllProduct();
                    break;
                // Add more cases here for future product types
                case "GetProductsByKeyword":
                    if(str == "") products = proRepo.GetAllProduct();
                    else products = proRepo.GetProductsByKeyword(str);
                    break;
                default:
                    // Default to "All" if no valid productType is passed
                    products = proRepo.GetAllProduct();
                    break;
            }

            return View(products);
        }
    }

}
