namespace SWP391_FinalProject.Models
{
    public class ProductNameLogModel
    {
        public string Id { get; set; }

        public string ProductItemId { get; set; }

        public string OldName { get; set; }

        public string NewName { get; set; }

        public DateTime ChangeTimeStamp { get; set; }
    }
}
