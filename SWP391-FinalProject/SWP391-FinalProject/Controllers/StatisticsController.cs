using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult OrderStat()
        {
            StatisticsRepository statRepo = new StatisticsRepository();
            dynamic result = statRepo.GetOrderStat();
            ViewBag.OrderStat = result;
            return View();
        }
    }
}
