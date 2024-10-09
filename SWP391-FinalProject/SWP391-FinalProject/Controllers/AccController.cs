using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class AccController : Controller
    {
        private readonly DBContext db;

        public AccController(DBContext context)
        {
            db= context;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            Repository.Province provinceRepo = new Repository.Province(db);
            var province = provinceRepo.GetAllProvince();
            ViewBag.Provinces = province;
            return View();
        }

        [HttpPost]
        public IActionResult Register(Models.AccountModel model)
        {
            Repository.Account accRepo = new Repository.Account(db); 
            accRepo.AddAccount(model);
            return RedirectToAction("Index", "Pro");
        }
    }
}
