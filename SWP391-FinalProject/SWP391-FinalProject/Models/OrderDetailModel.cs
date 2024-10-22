namespace SWP391_FinalProject.Models
{
    public class OrderDetailModel
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public int StateId { get; set; }
        public DateTime Date { get; set; }
        public decimal UsePoint { get; set; }
        public decimal EarnPoint { get; set; }
        public string StaffShiftId { get; set; }
        public string ProductItemId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public string UserName { get; set; }
        public string UserProvince { get; set; }
        public string UserDistrict { get; set; }
        public string UserAddress { get; set; }
        public string StateName { get; set; }
    }
}
