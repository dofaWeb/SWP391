namespace SWP391_FinalProject.Repository;
using Microsoft.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
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
    public void DeleteComment(string id)
    {
        using (DBContext dbContext = new DBContext())
        {
            // Find the comment by ID
            var comment = dbContext.Comments.Find(id);
            if (comment != null)
            {
                // Remove the comment from the database
                dbContext.Comments.Remove(comment);
                dbContext.SaveChanges(); // Save changes to the database
            }
           
     
        }
    }
    public void UpdateComment(string id, string comment)
    {
        var existingComment = db.Comments.FirstOrDefault(c => c.Id == id);
        if (existingComment != null)
        {
            existingComment.Comment1 = comment; // Assuming Comment1 is the property holding the comment text
            db.SaveChanges();
        }
    }
    public List<CommentModel> GetCommentsByProductId(string productId)
    {
        using (DBContext dbContext = new DBContext())
        {
            var comments = (from c in dbContext.Comments
                            join a in dbContext.Accounts on c.UserId equals a.Id
                            join u in dbContext.Users on c.UserId equals u.AccountId
                            where c.ProductId == productId
                            select new CommentModel
                            {
                                Id = c.Id,
                                Comment = c.Comment1,
                                Date = c.Date,
                                UserName = a.Username,
                                FullName=u.Name
                            }).ToList();
            return comments;
        }
    }
    public List<Models.CommentModel> GetAllComments()
    {
        var comments = (from c in db.Comments
                        join u in db.Users on c.UserId equals u.AccountId
                       join p in db.Products on c.ProductId equals p.Id
                        select new CommentModel
                        {
                            Id = c.Id,
                            Product=new ProductModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Picture = p.Picture,
                                Description = p.Description,
                                CategoryId = p.CategoryId,
                                CategoryName = db.Categories
                                                   .Where(c => c.Id == p.CategoryId)
                                                   .Select(c => c.Name).FirstOrDefault(), // Lấy tên danh mục

                                StateId = p.StateId,
                            },

                            Comment = c.Comment1,
                            Date = c.Date,
                            UserName = u.Name // Getting the full name of the user directly from the join
                        }).OrderByDescending(c => c.Date)
                        .ToList(); // Convert IQueryable to List

        return comments;
    }
}

