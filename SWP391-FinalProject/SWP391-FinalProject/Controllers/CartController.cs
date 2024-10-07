using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class CartController : Controller
    {
        public IActionResult AddToCart(string id)
        {
            return View();
        }
    }
}
