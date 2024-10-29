using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;
using System.Security.Claims;

namespace SWP391_FinalProject.Controllers
{
    public class AccController : Controller
    {

        public AccController()
        {

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            HttpClient client = new HttpClient();
            try
            {
                string url = "https://esgoo.net/api-tinhthanh/1/0.htm";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseJson);
                    int error = jsonResponse.error;

                    if (error == 0)
                    {
                        // Get the "data" array from the JSON response
                        var data = jsonResponse.data;

                        // Pass the data to the view using ViewBag
                        ViewBag.Provinces = data;

                        // Render the Register view
                        return View("Register");
                    }
                    else
                    {
                        // Handle the error from the API
                        ViewBag.Error = jsonResponse.error_text;
                        return View("Register");
                    }
                }
                else
                {
                    // Handle non-success HTTP response
                    ViewBag.Error = "Error occurred while sending the request to the API: " + (int)response.StatusCode;
                    return View("Register");
                }
            }
            catch (Exception e)
            {
                // Handle any exceptions
                ViewBag.Error = "An error occurred while processing the request: " + e.Message;
                return View("Register");
            }
            finally
            {
                client.Dispose();
            }
        }


        public async Task<IActionResult> History()

        {
            string username = Request.Cookies["Username"];
            UserRepository userRepo = new UserRepository();
            string userId = userRepo.GetUserIdByUserName(username);
            return RedirectToAction("UserOrderHistory", "Order", new { UserId = userId });
        }

        public void AddRegisterInfoToCookie(AccountModel model)
        {
            CookieOptions Cookie = new CookieOptions();
            Cookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Append("RegisterCookie", model.Username + "/" + model.Password + "/" + model.Name + "/" + model.Email + "/" + model.Phone + "/" + model.Province + "/" + model.District + "/" + model.Address, Cookie);
        }

        [HttpPost]
        public async Task<IActionResult> Register(Models.AccountModel model)
        {
            Repository.AccountRepository accRepo = new Repository.AccountRepository();
            ViewBag.Error = "";
            if (!accRepo.CheckEmail(model.Email) || !accRepo.CheckUsername(model.Username))
            {
                if (!accRepo.CheckEmail(model.Email))
                {
                    ViewBag.Error = "Email has been used.";
                }
                if (!accRepo.CheckUsername(model.Username))
                {
                    ViewBag.Error += "<br>Username has been used.";
                }
                return await Register();
            }
            else
            {
                AddRegisterInfoToCookie(model);
                Helpers.MailUtil.SendRegisterEmail(model.Email);
                return RedirectToAction(nameof(SuccessfullySendEmail), new { email = model.Email });
            }

        }



        public IActionResult ReceiveRegisterEmail()
        {
            string cookie = Request.Cookies["RegisterCookie"];
            string[] model = cookie.Split("/");
            Repository.AccountRepository accRepo = new Repository.AccountRepository();
            AccountModel acc = new AccountModel() { Username = model[0], Password = model[1], Name = model[2], Email = model[3], Phone = model[4], Province = model[5], District = model[6], Address = model[7] };
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
                Repository.AccountRepository accRepo = new Repository.AccountRepository();
                if (accRepo.CheckEmail(email))
                {
                    AccountModel acc = new AccountModel() { Username = email, Email = email, Name = name, Password = "", Phone = "", Province = "", District = "", Address = "" };
                    accRepo.AddAccount(acc);

                    return await LoginByGoogle(email);
                }
                else
                {
                    return await LoginByGoogle(email);

                }
            }
            return RedirectToAction("Index", "Pro");

        }



        public async Task<IActionResult> LoginByGoogle(string email)
        {
            Repository.AccountRepository accRepo = new Repository.AccountRepository();
            var user = accRepo.GetUserByUsernameOrEmail(email);
            if (user == null)
            {
                ViewBag.Error = "This email has been used for register the staff";
                return View("Login");
            }
            if (user.Status == "Inactive")
            {
                ViewBag.Error = "Your account has been ban";
                return View("Login");
            }
            else
            {
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
                AddLoginCookie(user.Username);
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Pro");
            }
        }

        public bool CheckLoginCookie()
        {
            string cookie = HttpContext.Request.Cookies["Username"];
            return cookie != null && cookie.Length > 0;
        }

        public async Task<IActionResult> CheckIsLogin()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                if (CheckLoginCookie())
                {
                    HttpContext.Session.SetString("Username", HttpContext.Request.Cookies["Username"]);
                    return RedirectToAction("LoginWithCookie", "Acc", new { username = HttpContext.Request.Cookies["Username"] });
                }
            }
            return RedirectToAction("Index", "Pro");
        }



        [HttpPost]
        public async Task<IActionResult> Login(Models.AccountModel model)
        {
            Repository.AccountRepository accRepo = new Repository.AccountRepository();
            if (accRepo.Login(model.Username, model.Password))
            {
                var RoleId = accRepo.GetRoleId(model.Username);
                if (RoleId != "Role0003")
                {

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, RoleId),
                        new Claim(ClaimTypes.Name, model.Username),

                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);
                    return RedirectToAction("Index", "ProMan");
                }

                else
                {
                    var user = accRepo.GetUserByUsernameOrEmail(model.Username);


                    if (user.Status == "Inactive")
                    {
                        ViewBag.Error = "Your account has been banned";
                        return View();
                    }
                    else
                    {
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
                        AddLoginCookie(user.Username);
                        HttpContext.Session.SetString("Username", user.Username);
                        return RedirectToAction("Index", "Pro");
                    }
                }
            }
            else
            {
                ViewBag.Error = "Username or Password is invalid!";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoginWithCookie(string username)
        {
            AccountRepository accRepo = new AccountRepository();
            AccountModel acc = accRepo.GetUserByUsernameOrEmail(username);
            if (acc.Status == "Inactive")
            {
                ViewBag.Error = "Your account has been banned";
                return View("Login");
            }
            else
            {
                var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, acc.Email),
                                new Claim(ClaimTypes.Name, acc.Name),
                                new Claim(MySetting.CLAIM_CUSTOMERID, acc.Id),
                                new Claim(ClaimTypes.Role, acc.RoleId),
                                new Claim("Username", acc.Username)
                            };
                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return RedirectToAction("Index", "Pro");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string username, string email)
        {
            AccountRepository accRepo = new AccountRepository();
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
            if (MySetting.Otp != Otp)
            {
                ViewBag.Error = "Invalid Otp";
                return View();
            }
            else
            {
                MySetting.Otp = -99999;
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
            AccountRepository accRepo = new AccountRepository();
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


        void AddLoginCookie(string username)
        {
            CookieOptions Cookie = new CookieOptions();
            Cookie.Expires = DateTime.Now.AddDays(3);
            Response.Cookies.Append("Username", username, Cookie);

        }

        void DeleteLoginCookie()
        {
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("CartCookie");
            HttpContext.Session.Clear();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            DeleteLoginCookie();
            return RedirectToAction("Index", "Pro");
        }
    }
}
