using Microsoft.AspNetCore.Mvc;

namespace SWP391_FinalProject.Controllers
{
    public class CatManController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
