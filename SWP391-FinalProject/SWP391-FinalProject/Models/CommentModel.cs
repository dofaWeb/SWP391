namespace SWP391_FinalProject.Models
{
    public class CommentModel
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string ProductId { get; set; }
        public string UserName { get; set; }

        public string Comment {  get; set; }

        public DateTime Date {  get; set; }
    }
}
