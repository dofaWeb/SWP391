namespace SWP391_FinalProject.Models
{
    public class StaffShiftModel
    {
        public string Id { get; set; }
        public string Staff_Id { get; set; }

        public string Shift { get; set; }

        public decimal? HourlyRate { get; set; }

        public DateOnly Date { get; set; }

        public string Name { get; set; }
    }
}

