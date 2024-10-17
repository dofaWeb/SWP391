using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class CartController : Controller
    {
        public IActionResult AddToCart(string Ram, string Storage, string ProductId)
        {
            ProductRepository productRepository = new ProductRepository();
            string id = productRepository.GetProItemIdByVariation(Ram, Storage, ProductId);
            return View();
        }
    }
}
