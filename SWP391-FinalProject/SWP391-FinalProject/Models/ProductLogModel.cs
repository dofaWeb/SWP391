namespace SWP391_FinalProject.Models
{
    public class ProductLogModel
    {
        public string Id { get; set; }

        public string ProductItemId { get; set; }

        public string QuantityLogId { get; set; }

        public string PriceLogId { get; set; }

        public string DiscountLogId { get; set; }

        public string ChangeReasonId { get; set; }

        public string ActionType { get; set; }

        public DateTime Date { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }
    }
}
