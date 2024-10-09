using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Security.Claims;

namespace SWP391_FinalProject.Controllers
{
    public class AccController : Controller
    {
        private readonly DBContext db;

        public AccController(DBContext context)
        {
            db= context;
        }
        [HttpGet]
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
            if (accRepo.CheckEmail(model.Email))
            {
                accRepo.AddAccount(model);
                return RedirectToAction("Index", "Pro");
            }
            else
            {
                ViewBag.Error = "Email has been used!";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.AccountModel model)
        {
            Repository.Account accRepo = new Repository.Account(db);
            if (accRepo.Login(model.Username, model.Password))
            {
                var user = accRepo.GetUserByUsername(model.Username);
                var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.Name),
                                new Claim(MySetting.CLAIM_CUSTOMERID, user.Id),
                                new Claim(ClaimTypes.Role, user.RoleId)
                            };
                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(claimsPrincipal);
                return RedirectToAction("Index", "Pro");
            }
            return View();
        }
    }
}
