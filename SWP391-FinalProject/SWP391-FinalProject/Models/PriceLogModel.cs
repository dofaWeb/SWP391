namespace SWP391_FinalProject.Models
{
    public class PriceLogModel
    {
        public string Id { get; set; }

        public string ProductItemId { get; set; }

        public double OldePrice { get; set; }

        public double NewPrice { get; set; }

        public DateTime ChangeTimeStamp { get; set; }
    }
}
