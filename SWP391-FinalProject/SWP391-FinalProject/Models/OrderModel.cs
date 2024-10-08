namespace SWP391_FinalProject.Models
{
    public class OrderModel
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Addres { get; set; }

        public int StateId { get; set; }

        public DateTime Date { get; set; }

        public double UsePoint { get; set; }
        
        public double EarnPoint { get; set; }

        public string StaffShiftId { get; set; }


    }
}
