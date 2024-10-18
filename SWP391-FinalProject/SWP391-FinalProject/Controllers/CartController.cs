using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class CartController : Controller
    {
        public IActionResult AddToCart(string Option, string ProductId)
        {
            var parts = Option.Split(new string[] { "RAM: ", "<br/> Storage: " }, StringSplitOptions.None);


            string ram = parts[1];
            string storage = parts[2];
            ProductRepository productRepository = new ProductRepository();
            string ProductItemId= productRepository.GetProItemIdByVariation(ram, storage, ProductId);
            ProductItemRepository proItemRepo = new ProductItemRepository();
            ProductItemModel proItem = proItemRepo.getProductItemByProductItemId(ProductItemId);
            proItem.Ram = ram;
            proItem.Storage = storage;
            proItem.PriceAfterDiscount = ProductRepository.CalculatePriceAfterDiscount(proItem.SellingPrice, proItem.Discount);
            AddToCartCookie(proItem);
            return View();
        }

        public void AddToCartCookie(ProductItemModel proItem)
        {
            string cookie = getCartFromCookie();
            if (cookie == "")
            {
                CookieOptions Cookie = new CookieOptions();
                Cookie.Expires = DateTime.Now.AddDays(1);
                string CookieData = "=" + proItem.Id + "/" + proItem.Product.Id + "/" + proItem.Product.Name + "/" + proItem.Product.Picture + "/" + proItem.Product.Description + "/" + "1" + "/" + proItem.Quantity + "/" + proItem.SellingPrice + "/" + proItem.Discount + "/" + proItem.PriceAfterDiscount;
                Response.Cookies.Append("CartCookie", CookieData,Cookie);
            }
            else
            {

            }
        }

        public string getCartFromCookie()
        {
            
            string cookie = Request.Cookies["CartCookie"] ?? "";

            return cookie;
        }
    }
}
