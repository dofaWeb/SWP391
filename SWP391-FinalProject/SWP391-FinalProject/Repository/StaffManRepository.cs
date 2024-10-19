using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class StaffManRepository
    {
        private readonly DBContext db;
        public StaffManRepository()
        {
            db = new DBContext();
        }

        public int GetTotalHourWorked(string staffId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalHoursWorked = db.StaffShifts
                .Where(shift => shift.Date <= today && shift.StaffId == staffId)
                .Count() * 5;

            return totalHoursWorked;
        }

        public List<StaffModel> GetAllStaff()
        {
            var result = db.Staff.ToList().Select(p => new StaffModel
            {
                Name = p.Name,
                TotalHourWorked = GetTotalHourWorked(p.AccountId)
            }).ToList();
            return result;
        }
    }
}
