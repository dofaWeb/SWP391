using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class Order : Controller
    {
        public IActionResult Checkout()
        {
            return View();
        }

        public IActionResult ProcessCheckout() { 
            return View();
        }
    }
}
