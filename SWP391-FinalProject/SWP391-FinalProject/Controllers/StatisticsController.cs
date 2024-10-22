using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", "ProMan");
        }
    }
}
