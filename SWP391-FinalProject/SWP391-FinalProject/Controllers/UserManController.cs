using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;

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
            var user = db.Accounts.Select(p => new Models.AccountModel
            {
                Id = p.Id,
                Username = p.Username,
                Email = p.Email,
                Phone = p.Phone,
                RoleName = p.Role.Name,
                Status = (p.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
            });
            return View(user);
        }
    }
}
