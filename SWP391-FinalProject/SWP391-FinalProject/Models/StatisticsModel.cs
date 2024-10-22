namespace SWP391_FinalProject.Models
{
    public class StatisticsModel
    {
        public DateTime ChangeDate { get; set; }
        public string ProductItemId { get; set; }
        public int TotalQuantityAdded { get; set; }

        public decimal TotalImportPrice { get; set; }
    }
}
