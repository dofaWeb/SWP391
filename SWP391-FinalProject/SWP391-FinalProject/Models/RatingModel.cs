namespace SWP391_FinalProject.Models
{
    public class RatingModel
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string ProductItemId { get; set; }
        public string ProductId { get; set; }
        public int Rating { get; set; }

        public string Username { get; set; }
    }
}
