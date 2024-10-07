using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class ProController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ProductDetail(string id)
        {
            return View();
        }
    }
}
