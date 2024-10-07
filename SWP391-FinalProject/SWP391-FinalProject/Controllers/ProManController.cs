using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

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
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(object model) => RedirectToAction("Display");
    }
}
