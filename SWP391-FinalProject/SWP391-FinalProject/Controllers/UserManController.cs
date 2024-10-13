using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class UserManController : Controller
    {
        private readonly DBContext db;

        public UserManController(DBContext context)
        {
            db = context;
        }
        public IActionResult Display()

        {
            AccountRepository AccRepo = new AccountRepository(db);
            var user = AccRepo.GetAllAccount();
            return View(user);
        }
        public IActionResult ViewDetail(string id)

        {
            AccountRepository AccRepo = new AccountRepository(db);
            var user = AccRepo.GetAccountById(id);
            return View(user);
        }
    }
}

