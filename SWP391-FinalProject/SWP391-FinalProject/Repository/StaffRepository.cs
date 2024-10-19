using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

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

        public void SaveShiftData([FromBody] ShiftDataModel data)
        {
            // Step 1: Retrieve the last ID only once
            string lastId = db.StaffShifts
                              .OrderByDescending(a => a.Id)
                              .Select(a => a.Id)
                              .FirstOrDefault() ?? "S0000000"; // Start from "S0000000" if no record exists

            // Step 2: Generate new IDs sequentially
            List<Entities.StaffShift> newShiftRecords = new List<Entities.StaffShift>();
            int currentNumber = int.Parse(lastId.Substring(1)); // Extract the numeric part

            foreach (var shift in data.Shifts)
            {
                currentNumber++; // Increment the number for each new ID

                // Generate the new ID
                string newId = $"S{currentNumber:D7}"; // Format with leading zeros

                // Create the new shift record
                var newShiftRecord = new Entities.StaffShift
                {
                    Id = newId,
                    StaffId = GetStaffIdByName(shift.StaffId),
                    Shift = shift.Shift,
                    HourlyRate = data.HourlyRate,
                    Date = DateOnly.Parse(shift.Date) // Assuming you're using DateOnly type
                };

                newShiftRecords.Add(newShiftRecord); // Add to the list
            }

            // Step 3: Insert all new records into the database
            db.StaffShifts.AddRange(newShiftRecords);
            db.SaveChanges(); // Commit the changes
        }

    }
}
