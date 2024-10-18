using Microsoft.AspNetCore.Mvc;
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
            string id = productRepository.GetProItemIdByVariation(ram, storage, ProductId);
            return View();
        }
    }
}
