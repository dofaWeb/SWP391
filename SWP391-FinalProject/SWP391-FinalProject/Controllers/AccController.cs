using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            Repository.ProvinceRepository provinceRepo = new Repository.ProvinceRepository(db);
            var province = provinceRepo.GetAllProvince();
            ViewBag.Provinces = province;
            return View();
        }

        public void AddRegisterInfoToCookie(AccountModel model)
        {
            CookieOptions Cookie = new CookieOptions();
            Cookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Append("RegisterCookie", model.Username + "/" + model.Password + "/" + model.Name + "/" + model.Email + "/" + model.Phone + "/" + model.ProvinceId + "/" + model.Address, Cookie);
        }

        [HttpPost]
        public IActionResult Register(Models.AccountModel model)
        {
            Repository.AccountRepository accRepo = new Repository.AccountRepository(db);
            if (!accRepo.CheckEmail(model.Email))
            {
                ViewBag.Error = "Email has been used!";
                return Register();
            }
            else
            {
                AddRegisterInfoToCookie(model);
                Random random = new Random();
                Helpers.MailUtil.SendRegisterEmail(model.Email);
                return RedirectToAction(nameof(SuccessfullySendEmail), new { email = model.Email });
            }

        }

        public IActionResult ReceiveRegisterEmail()
        {
            string cookie = Request.Cookies["RegisterCookie"];
            string[] model = cookie.Split("/");
            Repository.AccountRepository accRepo = new Repository.AccountRepository(db);
            AccountModel acc = new AccountModel() { Username = model[0], Password = model[1], Name = model[2], Email = model[3], Phone = model[4], ProvinceId = model[5], Address = model[6] };
            accRepo.AddAccount(acc);
            Response.Cookies.Delete("RegisterCookie");
            return RedirectToAction("Login");
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
                Repository.AccountRepository accRepo = new Repository.AccountRepository(db);
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
            Repository.AccountRepository accRepo = new Repository.AccountRepository(db);
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
            Repository.AccountRepository accRepo = new Repository.AccountRepository(db);
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

        [HttpGet]
        public async Task<IActionResult> ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string username, string email)
        {
            AccountRepository accRepo = new AccountRepository(db);
            AccountModel AccModel = accRepo.GetUserByUsernameAndEmail(username, email);
            if (AccModel == null)
            {
                ViewBag.error = "Username or Email is invalid!";
                return View();
            }
            else
            {
                Random random = new Random();
                MySetting.Otp = random.Next(0, 9999);
                MySetting.Account = AccModel;
                await MailUtil.SendForgetPasswordEmail(email, MySetting.Otp);
                return RedirectToAction(nameof(EnterOtp));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EnterOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnterOtp(string OtpCode)
        {
            int Otp = int.Parse(OtpCode);
            if(MySetting.Otp != Otp)
            {
                ViewBag.Error = "Invalid Otp";
                return View();
            }
            else
            {
                return RedirectToAction(nameof(ResetPassword));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            ViewBag.Username = MySetting.Account.Username;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string password)
        {
            AccountRepository accRepo = new AccountRepository(db);
            AccountModel AccModel = MySetting.Account;
            AccModel.Password = password;
            accRepo.ResetPassword(AccModel);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> SuccessfullySendEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile(string username)
        {
            Repository.UserRepository userRepo = new Repository.UserRepository();
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
