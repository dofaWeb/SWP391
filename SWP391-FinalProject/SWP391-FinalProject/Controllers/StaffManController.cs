using Microsoft.AspNetCore.Mvc;
using System.Text;
using System;
using SWP391_FinalProject.Helpers;

namespace SWP391_FinalProject.Controllers
{
    public class StaffManController : Controller
    {
        public IActionResult Display()
        {
            return View();
        }

        public static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }


        public IActionResult CreateAccount(string StaffEmail)
        {
            MailUtil.SendForgetPasswordEmail(StaffEmail, 20);
            return View("Display");
        }
    }
}
