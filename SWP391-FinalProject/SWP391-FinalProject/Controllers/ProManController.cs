using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Net.Http.Headers;

namespace SWP391_FinalProject.Controllers
{
    public class ProManController : Controller
    {
        private readonly DBContext db;

        public ProManController(DBContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Display()
        {
            Repository.Product proRepo = new Repository.Product(db);
            var query = proRepo.GetAllProduct();

            return View(query);
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