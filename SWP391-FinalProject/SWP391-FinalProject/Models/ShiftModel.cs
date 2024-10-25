namespace SWP391_FinalProject.Models
{
    public class ShiftSchdeduleModel
    {
        public string Id { get; set; }
        public DateOnly? Date { get; set; }
        public string Shift { get; set; } // "Morning" or "Afternoon"
        public string StaffId { get; set; }
        public string StaffName { get; set; }
    }
}
