using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using System.Text;
using System;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Repository;
using SWP391_FinalProject.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SWP391_FinalProject.Controllers
{
    public class StaffManController : Controller
    {
        [HttpGet]
        public IActionResult StaffList(string date = "21/10/2024", int? page = 1)
        {
            StaffRepository staffrepo = new StaffRepository();

            var staff = staffrepo.GetAllStaff();

            var shifts = staffrepo.GetShiftData(date); // Get shift data

            int pageSize = 5; // Number of items per page
            int pageNumber = page ?? 1; // Current page number

            // Paginate the shift data
            var pagedShifts = shifts.AsQueryable().ToPagedList(pageNumber, pageSize);

            // Store pagination information in ViewBag
            ViewBag.PageCount = pagedShifts.PageCount;
            ViewBag.PageNumber = pagedShifts.PageNumber;
            ViewBag.HasPreviousPage = pagedShifts.HasPreviousPage;
            ViewBag.HasNextPage = pagedShifts.HasNextPage;

            ViewBag.StaffList = staff.Where(p=>p.Account.Status== "Available").Select(s => new { s.Id, s.Name }).ToList();

            ViewBag.Shift = pagedShifts; // Pass the paginated list to the view

            ViewBag.shifts = shifts;

            return View(staff); // Render the view
        }
        [HttpPost]
        public IActionResult StaffList(string keyword,string date = "21/10/2024", int? page = 1)
        {
            StaffRepository staffrepo = new StaffRepository();

            var staff = staffrepo.GetAllStaffByKeyword(keyword);

            var shifts = staffrepo.GetShiftData(date); // Get shift data

            int pageSize = 5; // Number of items per page
            int pageNumber = page ?? 1; // Current page number

            // Paginate the shift data
            var pagedShifts = shifts.AsQueryable().ToPagedList(pageNumber, pageSize);

            // Store pagination information in ViewBag
            ViewBag.PageCount = pagedShifts.PageCount;
            ViewBag.PageNumber = pagedShifts.PageNumber;
            ViewBag.HasPreviousPage = pagedShifts.HasPreviousPage;
            ViewBag.HasNextPage = pagedShifts.HasNextPage;

            ViewBag.StaffList = staff.Where(p => p.Account.Status == "Available").Select(s => new { s.Id, s.Name }).ToList();

            ViewBag.Shift = pagedShifts; // Pass the paginated list to the view

            ViewBag.shifts = shifts;

            return View(staff); // Render the view
        }

        public IActionResult EditSalary(string staffId, int staffSalary)
        {
            StaffRepository staffRepository = new StaffRepository();
            staffRepository.EditSalary(staffId, staffSalary);
            
            return RedirectToAction("StaffList");
        }

        public IActionResult Schedule(string Username)
        {
            StaffRepository staffRepository = new StaffRepository();
            AccountRepository accountRepository = new AccountRepository();
            var schedule = staffRepository.GetStaffSchedule(accountRepository.GetIdByUsername(Username));
            return View(schedule);
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
        public IActionResult StaffSetting(string username)
        {

            StaffRepository staffRepo= new StaffRepository();
            var result=staffRepo.GetStaffbyUserName(username);
            return View(result);    
        }

        public IActionResult CreateAccount(string StaffEmail)
        {
            string password = GenerateRandomString(10);
            
            AccountRepository accRepo = new AccountRepository();
            var check = accRepo.checkExistedAccount(StaffEmail);
            if (!check)
            {
                accRepo.AddStaffAccount(new Models.AccountModel
                {
                    Name = StaffEmail,
                    Email = StaffEmail,
                    Password = password,
                });
                MailUtil.SendRegisterStaffEmail(StaffEmail, StaffEmail, password);
                return RedirectToAction("StaffList");
            }
            else
            {
                TempData["Error"] = "This email already exists!";
                return RedirectToAction("StaffList");
            }
        }

        [HttpPost]
        public IActionResult EditStaffProfile(StaffModel staff)
        {
            StaffRepository staffRepo = new StaffRepository();
            staffRepo.UpdateStaff(staff);


            return RedirectToAction("StaffSetting", new { username = staff.Account.Email });
        }

        [HttpPost]
        public IActionResult EditShift(string shiftId, string staffId)
        {
            StaffRepository staffRepository = new StaffRepository();
            staffRepository.UpdateShift(shiftId, staffId);
            return RedirectToAction("StaffList");
        }

        [HttpPost]
        public IActionResult AddShift(DateOnly date, string staffIdMorning, string staffIdAfternoon)
        {
            StaffRepository staffRepository = new StaffRepository();
            staffRepository.AddShift(date, staffIdMorning, staffIdAfternoon);
            return RedirectToAction("StaffList");
        }

    }
}
