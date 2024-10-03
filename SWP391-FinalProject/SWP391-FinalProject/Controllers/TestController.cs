using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
