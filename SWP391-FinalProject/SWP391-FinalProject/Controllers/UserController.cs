﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using SWP391_FinalProject.Filters;
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
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
            //random picture profile
            var images = new List<string>
    {
                "/pictures/bg.jpg",
                "/pictures/bgg.jpg",
                "/pictures/day.png",
                "/pictures/ap1.jpeg",
                "/pictures/ap2.jpeg",
                "/pictures/ap3.jpeg",
                "/pictures/ap4.jpeg",
                "/pictures/ap5.jpg",
        
    };

            var random = new Random();
            int index = random.Next(images.Count);
            ViewBag.ProfilePicture = images[index];
            Repository.UserRepository userRepo = new Repository.UserRepository();
            UserModel user = new UserModel();
            user = userRepo.GetUserProfileByUsername(username);
            bool IsLoginWithGoogle = false;
            if (user.Account.Password.Equals(Helpers.MySetting.GetMd5Hash(""))){
                IsLoginWithGoogle = true;
            }
            ViewBag.IsLoginWithGoogle = IsLoginWithGoogle;
            return View(user);

        }
        [HttpGet]
        public async Task<IActionResult> EditProfile(string username)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
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

                        UserRepository UserRepo = new UserRepository();
                        UserModel User = UserRepo.GetUserProfileByUsername(username);
                        return View(User);
                    }
                    else
                    {
                        // Handle the error from the API
                        ViewBag.Error = jsonResponse.error_text;
                        //return View("Register");
                    }
                }
                else
                {
                    // Handle non-success HTTP response
                    ViewBag.Error = "Error occurred while sending the request to the API: " + (int)response.StatusCode;
                    //return View("Register");
                }
            }
            catch (Exception e)
            {
                // Handle any exceptions
                ViewBag.Error = "An error occurred while processing the request: " + e.Message;
                //return View("Register");
            }
            finally
            {
                client.Dispose();
            }
            return View(User);
        }

        [HttpPost]
        public IActionResult EditProfile(UserModel user)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
            UserRepository UserRepo = new UserRepository();
            UserRepo.UpdateUser(user);

            return RedirectToAction(nameof(Profile), new {username = user.Account.Username});
        }

        public IActionResult ChangePassword()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
            if (UserAuthorizationFilter.CheckUser(userId))
            {
                TempData["ErrorMessage"] = "Your account has been ban!";
                return RedirectToAction("Login", "Acc");
            }
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
