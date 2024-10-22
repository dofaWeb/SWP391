using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;


namespace SWP391_FinalProject.Controllers
{
    public class OrderManController : Controller
    {
        public IActionResult Display()
        {
            return View();
        }

    }
}
