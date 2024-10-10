using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
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
            db = context;
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

        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Acc");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal != null)
            {
                var claimsIdentity = (ClaimsIdentity)result.Principal.Identity;

                // Lấy thông tin người dùng từ claims
                var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
                var name = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
                Repository.Account accRepo = new Repository.Account(db);
                if (accRepo.CheckEmail(email))
                {
                    AccountModel acc = new AccountModel() { Username = email, Email = email, Name = name, Password = "", Phone = "", ProvinceId = "Prov0001", Address = "" };
                    accRepo.AddAccount(acc);

                    await LoginByGoogle(email);
                }
                else
                {
                    await LoginByGoogle(email);

                }
            }

            return RedirectToAction("Index", "Pro");
        }

        public async Task<IActionResult> LoginByGoogle(string email)
        {
            Repository.Account accRepo = new Repository.Account(db);
            var user = accRepo.GetUserByUsernameOrEmail(email);
            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.Name),
                                new Claim(MySetting.CLAIM_CUSTOMERID, user.Id),
                                new Claim(ClaimTypes.Role, user.RoleId),
                                new Claim("Username", user.Username)
                            };
            var claimsIdentity = new ClaimsIdentity(claims, "login");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(claimsPrincipal);
            if (user.Status == "0")
            {
                ViewBag.Error = "Your account has been ban";
                return View("Login");
            }
            else
            {
                return RedirectToAction("Index", "Pro");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.AccountModel model)
        {
            Repository.Account accRepo = new Repository.Account(db);
            if (accRepo.Login(model.Username, model.Password))
            {
                var RoleId = accRepo.GetRoleId(model.Username);
                if (RoleId != "Role0003")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, RoleId)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);
                    return RedirectToAction("Index", "ProMan");
                }

                else
                {
                    var user = accRepo.GetUserByUsernameOrEmail(model.Username);
                    var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.Name),
                                new Claim(MySetting.CLAIM_CUSTOMERID, user.Id),
                                new Claim(ClaimTypes.Role, user.RoleId),
                                new Claim("Username", user.Username)
                            };
                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);
                    if (user.Status == "0")
                    {
                        ViewBag.Error = "Your account has been banned";
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Index", "Pro");
                    }
                }


            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile(string username)
        {
            Repository.User userRepo = new Repository.User();
            UserModel user = new UserModel();
            user = userRepo.GetUserProfileByUsername(username);

            return View(user);

        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Pro");
        }
    }
}
