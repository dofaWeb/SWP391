﻿using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SWP391_FinalProject.Helpers;

namespace SWP391_FinalProject.Repository
{
    public class StaffRepository
    {
        private readonly DBContext db;

        public StaffRepository()
        {
            db = new DBContext();
        }

        public string GetNewId()
        {
            string lastId = db.StaffShifts
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "S0000001";
            }
            // Tách phần chữ (A) và phần số (0000001)
            string prefix = lastId.Substring(0, 1); // Lấy ký tự đầu tiên
            int number = int.Parse(lastId.Substring(1)); // Lấy phần số và chuyển thành số nguyên

            // Tăng số lên 1
            int newNumber = number + 1;

            // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }
        public int GetTotalHourWorked(string staffId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalHoursWorked = db.StaffShifts
                .Where(shift => shift.Date <= today && shift.StaffId == staffId)
                .Count();

            return totalHoursWorked * 5;  // If no shifts, it returns 0.
        }

        public int GetTotalOrder(string staffId)
        {
            var result = from ss in db.StaffShifts
                         join o in db.Orders on ss.Id equals o.StaffShiftId
                         where ss.StaffId == staffId
                         select o;

            return result.Count();
        }

        public List<StaffModel> GetAllStaff()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Batch query: Get total hours worked per staff.
            var totalHoursWorked = db.StaffShifts
                .Where(shift => shift.Date <= today)
                .GroupBy(shift => shift.StaffId)
                .Select(g => new
                {
                    StaffId = g.Key,
                    HoursWorked = g.Count() * 5  // Each shift is 5 hours.
                })
                .ToDictionary(x => x.StaffId, x => x.HoursWorked);

            // Batch query: Get total orders per staff.
            var totalOrders = db.StaffShifts
                .Join(db.Orders, ss => ss.Id, o => o.StaffShiftId, (ss, o) => new { ss.StaffId })
                .GroupBy(x => x.StaffId)
                .Select(g => new
                {
                    StaffId = g.Key,
                    OrderCount = g.Count()
                })
                .ToDictionary(x => x.StaffId, x => x.OrderCount);

            // Create the final list of staff models.
            var result = db.Staff.Where(p => p.Account.RoleId == "Role0002").Select(p => new StaffModel
            {
                Id = p.AccountId,
                Name = p.Name,
                Account = new AccountModel
                {
                    Status = (p.Account.IsActive == ulong.Parse("1")) ? "Available" : "Unavailable"
                },
                TotalHourWorked = totalHoursWorked.ContainsKey(p.AccountId) ? totalHoursWorked[p.AccountId] : 0,
                TotalPay = (totalHoursWorked.ContainsKey(p.AccountId) ? totalHoursWorked[p.AccountId] : 0) * p.Salary,
                TotalOrders = totalOrders.ContainsKey(p.AccountId) ? totalOrders[p.AccountId] : 0,
                AvgOrder = totalOrders.ContainsKey(p.AccountId) && totalHoursWorked.ContainsKey(p.AccountId)
                           ? (double)totalOrders[p.AccountId] / (totalHoursWorked[p.AccountId] / 5)
                           : 0,
                TotalMoneyMade = 20
            }).ToList();

            return result;
        }

        public string GetStaffIdByName(string name)
        {
            var result = db.Staff.Where(p => p.Name == name).Select(p => p.AccountId).FirstOrDefault();
            return result;
        }

        // Helper function to check if two dates belong to the same week
        private bool IsSameWeek(DateOnly date1, DateOnly date2)
        {
            var startOfWeek1 = GetMondayOfWeek(date1);
            var startOfWeek2 = GetMondayOfWeek(date2);
            return startOfWeek1 == startOfWeek2;
        }
        public StaffModel GetStaffbyUserName(string userName)
        {
            var result = (from s in db.Staff
                          join a in db.Accounts on s.AccountId equals a.Id
                          where a.Username == userName
                          select new StaffModel
                          {
                              Id = s.AccountId,
                              Name = s.Name,
                              Account = new AccountModel
                              {
                                  Password = a.Password,
                                  Email=a.Email,
                                  RoleId=a.RoleId,
                                 
                              },
                              Salary=s.Salary
                          }).FirstOrDefault();

            return result;
        }
        public void UpdateStaff(Models.StaffModel staff)
        {
            var existingStaff= db.Staff.FirstOrDefault(s => s.AccountId == staff.Id);
            
            if (existingStaff != null) {
                if (staff.Name != null)
                {
                    existingStaff.Name = staff.Name;
                }
                if (staff.Account.Password != null) {
                    AccountRepository accRepo = new AccountRepository();
                    AccountModel Acc = accRepo.GetStaffByUsernameOrEmail("tek83522@gmail.com");
                  


               
                    Acc.Password = staff.Account.Password;
                    accRepo.ResetPassword(Acc);


                }
                db.SaveChanges();
            }

            
        }
        private DateOnly GetMondayOfWeek(DateOnly date)
        {
            int dayOffset = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (dayOffset < 0) dayOffset += 7;
            return date.AddDays(-dayOffset);
        }

        public string GetShiftIdByDateAndShift(DateOnly Date, string shift)
        {
            var id = db.StaffShifts.Where(p => p.Date == Date && p.Shift == shift).Select(p => p.Id).FirstOrDefault();
            return id;
        }

        public void EditShiftDate(ShiftDataModel model)
        {

            DateTime currentDate = DateTime.Today; // Get today's date

            foreach (var item in model.Shifts)
            {
                // Parse the date string from the ShiftModel to a DateTime object
                DateTime shiftDate = DateTime.Parse(item.Date); // Assumes the date is in "YYYY-MM-DD" format
                if (shiftDate > currentDate)
                {
                    string id = GetShiftIdByDateAndShift(DateOnly.Parse(item.Date), item.Shift);
                    if (id != null && item.StaffId != "Select the staff")
                    {
                        var newShiftRecord = new Entities.StaffShift
                        {
                            Id = id,
                            StaffId = GetStaffIdByName(item.StaffId),
                            Shift = item.Shift,
                            Date = DateOnly.Parse(item.Date), // Handle DateOnly correctly
                            HourlyRate = model.HourlyRate
                        };
                        db.StaffShifts.Update(newShiftRecord);
                    }
                }
            }
            db.SaveChanges();
        }

        public string SaveShiftData(ShiftDataModel data)
        {
            // Log start of save operation


            // Get the earliest shift date (start of the week)
            var shiftDates = data.Shifts.Select(shift => DateOnly.Parse(shift.Date));
            var weekStartDate = GetMondayOfWeek(shiftDates.Min());

            // Check if shifts already exist
            bool shiftsExist = db.StaffShifts
                .Where(s => shiftDates.Contains(s.Date.Value))
                .Any();

            if (shiftsExist)
            {
                return ("Shifts for this week already exist in the database.");
            }

            // Generate IDs and create new shift records
            string lastId = db.StaffShifts
                .OrderByDescending(a => a.Id)
                .Select(a => a.Id)
                .FirstOrDefault() ?? "S0000000";

            int currentNumber = int.Parse(lastId.Substring(1));
            List<Entities.StaffShift> newShiftRecords = new List<Entities.StaffShift>();

            foreach (var shift in data.Shifts)
            {
                currentNumber++;
                string newId = $"S{currentNumber:D7}";

                var newShiftRecord = new Entities.StaffShift
                {
                    Id = newId,
                    StaffId = GetStaffIdByName(shift.StaffId),
                    Shift = shift.Shift,
                    HourlyRate = data.HourlyRate,
                    Date = DateOnly.Parse(shift.Date) // Handle DateOnly correctly
                };

                newShiftRecords.Add(newShiftRecord);
            }

            // Insert new records and save changes
            db.StaffShifts.AddRange(newShiftRecords);
            db.SaveChanges(); // Uncomment this line to actually save the changes
            return "Save successfully";
        }

        public string GetStaffNameById(string id)
        {
            var name = db.Staff.Where(p => p.AccountId == id).Select(p => p.Name).FirstOrDefault();
            return name;
        }

        public dynamic GetAllStaffUpdate()
        {
            var staffList = db.Staff.Select(s => new
            {
                Id = s.AccountId,
                Name = s.Name
            }).ToList();
            return staffList;
        }

        public List<ShiftSchdeduleModel> GetShiftData(string weekStartDate)
        {
            var startDate = DateOnly.Parse(weekStartDate);
            var endDate = startDate.AddDays(4); // Monday to Friday

            // Query for morning shifts
            var shiftsMorning = db.StaffShifts
                .Where(shift => shift.Date >= startDate && shift.Date <= endDate)
                .Where(shift => shift.Shift == "Morning")
                .Select(shift => new ShiftSchdeduleModel
                {
                    Date = shift.Date.HasValue ? shift.Date.Value.ToString("yyyy-MM-dd") : string.Empty,
                    Shift = shift.Shift,
                    StaffId = shift.StaffId,
                    StaffName = shift.Staff.Name // Assuming Staff relationship exists
                })
                .ToList();

            // Query for afternoon shifts
            var shiftsAfternoon = db.StaffShifts
                .Where(shift => shift.Date >= startDate && shift.Date <= endDate)
                .Where(shift => shift.Shift == "Afternoon")
                .Select(shift => new ShiftSchdeduleModel
                {
                    Date = shift.Date.HasValue ? shift.Date.Value.ToString("yyyy-MM-dd") : string.Empty,
                    Shift = shift.Shift,
                    StaffId = shift.StaffId,
                    StaffName = shift.Staff.Name // Assuming Staff relationship exists
                })
                .ToList();

            // Merge both lists
            var shifts = shiftsMorning.Concat(shiftsAfternoon).ToList();

            return shifts;
        }



    }
}
