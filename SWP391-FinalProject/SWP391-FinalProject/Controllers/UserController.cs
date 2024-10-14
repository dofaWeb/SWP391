using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Controllers
{
    public class UserController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Profile(string username)
        {
            Repository.UserRepository userRepo = new Repository.UserRepository();
            UserModel user = new UserModel();
            user = userRepo.GetUserProfileByUsername(username);

            return View(user);

        }
        public IActionResult EditProfile(string username)
        {
            UserRepository UserRepo = new UserRepository();
            UserModel User = UserRepo.GetUserProfileByUsername(username);
            return View(User);
        }

        [HttpPost]
        public IActionResult EditProfile(UserModel user)
        {
            UserRepository UserRepo = new UserRepository();
            UserRepo.UpdateUser(user);

            return RedirectToAction(nameof(Profile), new {username = user.Account.Username});
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string username, string password)
        {
            AccountRepository accRepo = new AccountRepository();
            AccountModel accountModel = accRepo.GetUserByUsernameOrEmail(username);
            password = MySetting.GetMd5Hash(password);
            if (password != accountModel.Password)
            {
                ViewBag.Error = "Invalid password!";
                return View();
            }
            else
            {
                MySetting.Account = accountModel;
                return RedirectToAction(nameof(ResetPassword),"Acc");
            }
        }
    }
}
