using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class ProManController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Display()
        {
            return View();
        }
    }
}
