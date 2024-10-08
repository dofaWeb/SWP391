namespace SWP391_FinalProject.Models
{
    public class DiscountLogModel
    {
        public string Id { get; set; }

        public string ProductItemId { get; set; }

        public double OldDiscount { get; set; }

        public double NewDiscount { get; set; }

        public DateTime ChangeTimeStamp { get; set; }
    }
}
