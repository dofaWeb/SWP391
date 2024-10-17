namespace SWP391_FinalProject.Repository;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;

public class ComRepository
{
    private readonly DBContext db;

    public ComRepository()
    {
        db = new DBContext();
    }
    public string getNewCommentId()
    {
        string lastId = db.Comments
                 .OrderByDescending(a => a.Id)
                 .Select(a => a.Id)
                 .FirstOrDefault();
        if (lastId == null)
        {
            return "C0000001";
        }
        // Tách phần chữ (A) và phần số (0000001)
        string prefix = lastId.Substring(0, 1); // Lấy ký tự đầu tiên
        int number = int.Parse(lastId.Substring(1)); // Lấy phần số và chuyển thành số nguyên

        // Tăng số lên 1
        int newNumber = number + 1;

        // Tạo ID mới với số đã tăng, định dạng lại với 7 chữ số
        string newId = $"{prefix}{newNumber:D7}";

        return newId;
    }

    public void AddComment(CommentModel model)
    {
        model.Id=getNewCommentId();
        model.Date = DateTime.Now;
        var newComment = new Entities.Comment
        {
            Id=model.Id,
            ProductId=model.ProductId,
            UserId=model.UserId,
            Date=model.Date,
            Comment1 = model.Comment,
           
        };
        db.Comments.Add(newComment);
        db.SaveChanges();
    }
    public List<CommentModel> GetCommentsByProductId(string productId)
    {
        using (DBContext dbContext = new DBContext())
        {
            return dbContext.Comments
                            .Where(c => c.ProductId == productId)
                            .Select(c => new CommentModel
                            {
                                Id = c.Id,
                                Comment = c.Comment1,
                                Date = c.Date,
                                UserName = dbContext.Users
                                            .Where(u => u.AccountId == c.UserId)
                                            .Select(u => u.Name).FirstOrDefault()
                            }).ToList();
        }
    }
}

