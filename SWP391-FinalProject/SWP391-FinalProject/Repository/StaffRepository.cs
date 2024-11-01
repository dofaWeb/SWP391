using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SWP391_FinalProject.Helpers;
using System.Data;

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
            // Query to get the latest ID in descending order
            string query = "SELECT Id FROM Staff_Shift ORDER BY Id DESC LIMIT 1";
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if result is empty, meaning no records are found
            string lastId = result.Rows.Count > 0 ? result.Rows[0]["Id"].ToString() : null;

            if (lastId == null)
            {
                return "S0000001";
            }

            // Extract the prefix and number from the last ID
            string prefix = lastId.Substring(0, 1); // Get the first two characters
            int number = int.Parse(lastId.Substring(1)); // Parse the numeric part of the ID

            // Increment the number by 1
            int newNumber = number + 1;

            // Format the new ID with the incremented number
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }
        public dynamic GetStaffSchedule(string staffId)
        {
            // SQL query to retrieve the schedule for the specified staff member
            string query = @"
    SELECT 
        Date,
        Shift
    FROM 
        Staff_Shift
    WHERE 
        Staff_Id = @StaffId;";

            // Create a dictionary to pass parameters
            var parameters = new Dictionary<string, object>
    {
        { "@StaffId", staffId }
    };

            // Execute the query and get the result as a DataTable
            DataTable scheduleTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Prepare the list to store the schedule results
            var schedule = new List<ShiftSchdeduleModel>();

            // Convert each DataRow to a ShiftScheduleModel object
            foreach (DataRow row in scheduleTable.Rows)
            {
                schedule.Add(new ShiftSchdeduleModel
                {
                    Date = DateOnly.FromDateTime((DateTime)row["Date"]),  // Convert DateTime to DateOnly
                    Shift = (string)row["Shift"]
                });

            }

            return schedule;
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
                
                TotalOrders = totalOrders.ContainsKey(p.AccountId) ? totalOrders[p.AccountId] : 0,
                AvgOrder = totalOrders.ContainsKey(p.AccountId) && totalHoursWorked.ContainsKey(p.AccountId)
                           ? (double)totalOrders[p.AccountId] / (totalHoursWorked[p.AccountId] / 5)
                           : 0,
                Salary = p.Salary
            }).ToList();

            return result;
        }

        public string GetStaffIdByName(string name)
        {
            var result = db.Staff.Where(p => p.Name == name).Select(p => p.AccountId).FirstOrDefault();
            return result;
        }

        public void UpdateShift(string shiftId, string staffId)
        {
            var shift = db.StaffShifts.Where(p => p.Id == shiftId).FirstOrDefault();
            shift.StaffId = staffId;
            db.SaveChanges();
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
                                  Email = a.Email,
                                  RoleId = a.RoleId,

                              },
                              Salary = s.Salary
                          }).FirstOrDefault();

            return result;
        }
        public void UpdateStaff(Models.StaffModel staff)
        {
            var existingStaff = db.Staff.FirstOrDefault(s => s.AccountId == staff.Id);

            if (existingStaff != null)
            {
                if (staff.Name != null)
                {
                    existingStaff.Name = staff.Name;
                }
                if (staff.Account.Password != null)
                {
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

        public List<ShiftSchdeduleModel> GetShiftData(string weekStartDate)
        {
            var shifts = db.StaffShifts
                .OrderByDescending(shift => shift.Date) // First order by Date
                .ThenBy(shift => shift.Shift == "morning" ? 0 : 1) // Sort morning shifts before afternoon
                .Include(shift => shift.Staff) // Include the Staff relation
                .Select(shift => new ShiftSchdeduleModel
                {
                    Id = shift.Id,
                    Date = shift.Date,
                    Shift = shift.Shift,
                    StaffId = shift.StaffId,
                    StaffName = shift.Staff.Name
                })
                .ToList();



            return shifts;
        }

        public void AddShift(DateOnly date, string staffIdMoring, string staffIdAfternoon)
        {
            db.StaffShifts.Add(new StaffShift
            {
                Id = GetNewId(),
                Date = date,
                Shift = "Morning",
                StaffId = staffIdMoring
            });

            db.SaveChanges();

            db.StaffShifts.Add(new StaffShift
            {
                Id = GetNewId(),
                Date = date,
                Shift = "Afternoon",
                StaffId = staffIdAfternoon
            });
            db.SaveChanges();
        }

    }
}
