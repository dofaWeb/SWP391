using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class CatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
