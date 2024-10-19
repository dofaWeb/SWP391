namespace SWP391_FinalProject.Models
{
    public class StaffModel
    {
        public AccountModel Account { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public double Salary { get; set; }

        public int TotalHourWorked { get; set; }

        public int TotalPay { get; set; }

        public int TotalOrders { get; set; }

        public double AvgOrder { get; set; }

        public int TotalMoneyMade { get; set; }
    }
}

