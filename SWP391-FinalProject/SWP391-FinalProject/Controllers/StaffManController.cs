using Microsoft.AspNetCore.Mvc;
using System.Text;
using System;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Repository;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Controllers
{
    public class StaffManController : Controller
    {
        public IActionResult Display()
        {
            StaffRepository staffRepository = new StaffRepository();
            var staff = staffRepository.GetAllStaff();
            return View(staff);
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
            string password = GenerateRandomString(10);
            MailUtil.SendRegisterStaffEmail(StaffEmail, StaffEmail, password);
            AccountRepository accRepo = new AccountRepository();
            accRepo.AddStaffAccount(new Models.AccountModel
            {
                Name = StaffEmail,
                Email = StaffEmail,
                Password = password,
            });


            return RedirectToAction("Display");
        }

        [HttpPost]
        public IActionResult SaveShiftData([FromBody] ShiftDataModel data)
        {
            try
            {
                StaffRepository staffRepository = new StaffRepository();
                staffRepository.SaveShiftData(data);
                return Json(new { success = true, message = "Data saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
