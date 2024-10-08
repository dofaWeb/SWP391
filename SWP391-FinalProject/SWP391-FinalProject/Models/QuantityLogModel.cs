namespace SWP391_FinalProject.Models
{
    public class QuantityLogModel
    {
        public string Id { get; set; }

        public string ProductItemId { get; set; }

        public int OldQuantity { get; set; }

        public int NewQuantity { get; set; }

        public DateTime ChangeTimeStamp { get; set; }
    }
}
