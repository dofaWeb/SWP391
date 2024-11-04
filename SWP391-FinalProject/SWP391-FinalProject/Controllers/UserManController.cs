using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Repository;
using System.Runtime.Intrinsics.Arm;

namespace SWP391_FinalProject.Controllers
{
    public class UserManController : Controller
    {

        public UserManController()
        {

        }
        [Authorize]
        public IActionResult Display()

        {
            AccountRepository AccRepo = new AccountRepository();
            var user = AccRepo.GetAllAccount();
            return View(user);
        }
        [Authorize]

        public IActionResult ViewDetail(string id)
        {
            AccountRepository AccRepo = new AccountRepository();
            var user = AccRepo.GetAccountById(id);
            return View(user);
        }
        [Authorize]

        public IActionResult Ban(string id)
        {
            UserRepository userRepo = new UserRepository();
            userRepo.BanUserById(id);
            return RedirectToAction("Display");
        }
        
    }
}

