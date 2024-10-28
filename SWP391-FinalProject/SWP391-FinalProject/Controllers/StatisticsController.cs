using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            StatisticsRepository statRepo = new StatisticsRepository();
            dynamic result = statRepo.GetOrderStat();
            ViewBag.OrderStat = result;
            var sellingProducts = statRepo.GetBestSellingProducts();
       
            var sellingBrands = statRepo.GetBestSellingBrands();
            var spendingCustomer = statRepo.GetMostSpendingCustomers();

            ViewBag.SellingProducts = sellingProducts;
            
            ViewBag.SellingBrands = sellingBrands;
            ViewBag.SpendingCustomers = spendingCustomer;
            return View();
        }
    }
}
