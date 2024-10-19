namespace SWP391_FinalProject.Models
{
    public class ShiftDataModel
    {
        public decimal HourlyRate { get; set; }
        public List<ShiftModel> Shifts { get; set; }
    }

    public class ShiftModel
    {
        public string StaffId { get; set; }
        public string Shift { get; set; } // "Morning" or "Afternoon"
        public string Date { get; set; }  // In "YYYY-MM-DD" format
    }
}