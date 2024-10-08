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
            return View();
        }

        [HttpPost]
        public IActionResult Register(Models.AccountModel model)
        {
            Repository.Account accRepo = new Repository.Account(db); 
            string id = accRepo.GetNewId();
            var newAccount = new SWP391_FinalProject.Entities.Account()
            {
                Id = id,
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = ulong.Parse("1"),
                RoleId = "R0000003",
            };

            db.Accounts.Add(newAccount);
            db.SaveChanges();
            return RedirectToAction("Index", "Pro");
        }
    }
}
