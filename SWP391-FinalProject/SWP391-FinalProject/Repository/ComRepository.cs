namespace SWP391_FinalProject.Repository;

using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using System.Data;

public class ComRepository
{
    private readonly DBContext db;

    public ComRepository()
    {
        db = new DBContext();
    }
    public List<CommentModel> SearchedComment(string keyword, DateTime? fromDate, DateTime? toDate)
    {
        // Start querying comments
        var comments = from c in db.Comments
                       join u in db.Users on c.UserId equals u.AccountId
                       join p in db.Products on c.ProductId equals p.Id
                      
                       select new CommentModel
                       {
                           Id = c.Id,
                           Product = new ProductModel
                           {
                               Id = p.Id,
                               Name = p.Name,
                               Picture = p.Picture,
                               Description = p.Description,
                               CategoryId = p.CategoryId,
                               CategoryName = db.Categories
                                                  .Where(cat => cat.Id == p.CategoryId)
                                                  .Select(cat => cat.Name).FirstOrDefault(),
                               StateId = p.StateId,
                           },
                           Replies = db.ReplyComments
                                    .Where(rc => rc.CommentId == c.Id)
                                    .Select(rc => new ReplyCommentModel
                                    {
                                        Id = rc.Id,
                                        CommentId = rc.CommentId,
                                        Comment = rc.Comment,
                                        Date = rc.Date,

                                    }).ToList(),
                           Comment = c.Comment1,
                           Date = c.Date,
                           UserName = u.Name,
                           UserId=c.UserId,
                           
                       };

        // Apply keyword filtering only if the keyword is not empty
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            comments = comments.Where(c => c.UserName.Contains(keyword) || c.UserId.Contains(keyword));
        }

        // Filter by date range if fromDate or toDate are provided
        if (fromDate.HasValue)
        {
            comments = comments.Where(c => c.Date >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            // Include all comments from the toDate by using the end of that day
            comments = comments.Where(c => c.Date <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        // Convert to a list and return
        return comments.OrderByDescending(c => c.Date).ToList();
    }


    public string GetNewCommentId()
    {
        // Define the prefix for comment IDs
        string prefix = "C"; // Assuming comment IDs start with "C"

        // Query to get the last ID from the Comments table ordered by descending ID
        string query = $"SELECT Id FROM Comment WHERE Id LIKE '{prefix}%' ORDER BY Id DESC LIMIT 1;";

        // Execute the query and get the result
        DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

        // Default to "C0000001" if no existing ID found
        if (resultTable.Rows.Count == 0)
        {
            return $"{prefix}0000001";
        }

        // Get the last ID from the result
        string lastId = resultTable.Rows[0]["Id"].ToString();

        // Extract the numeric part and increment
        int number = int.Parse(lastId.Substring(prefix.Length));
        int newNumber = number + 1;

        // Generate new ID formatted to 7 digits
        string newId = $"{prefix}{newNumber:D7}";

        return newId;
    }

    public string GetNewReplyId()
    {
        // Define the prefix for reply IDs
        string prefix = "R"; // Assuming reply IDs start with "R"

        // Query to get the last ID from the ReplyComments table ordered by descending ID
        string query = $"SELECT Id FROM  Reply_Comment WHERE Id LIKE '{prefix}%' ORDER BY Id DESC LIMIT 1;";

        // Execute the query and get the result
        DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

        // Default to "R0000001" if no existing ID found
        if (resultTable.Rows.Count == 0)
        {
            return $"{prefix}0000001";
        }

        // Get the last ID from the result
        string lastId = resultTable.Rows[0]["Id"].ToString();

        // Extract the numeric part and increment
        int number = int.Parse(lastId.Substring(prefix.Length));
        int newNumber = number + 1;

        // Generate new ID formatted to 7 digits
        string newId = $"{prefix}{newNumber:D7}";

        return newId;
    }

    public void AddComment(CommentModel model)
    {
        // Generate a new comment ID
        model.Id = GetNewCommentId();

        // Set the current date and time for the comment
        model.Date = DateTime.Now;

        // Prepare the SQL query for inserting a new comment
        string insertCommentQuery = @"
        INSERT INTO Comment (Id, product_id, user_id,  date, Comment) 
        VALUES (@Id, @product_id, @user_id, @Date, @Comment);
    ";

        // Execute the query to insert the new comment
        var result = DataAccess.DataAccess.ExecuteNonQuery(insertCommentQuery, new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@product_id", model.ProductId },
        { "@user_id", model.UserId },
        { "@Date", model.Date },
        { "@Comment", model.Comment } // Make sure this matches the property in your CommentModel
    });

        // Optionally, you could check the result for success/failure
    }

    public void AddReply(ReplyCommentModel model)
    {
        // Generate a new reply ID
        model.Id = GetNewReplyId();

        // Set the current date and time for the reply
        model.Date = DateTime.Now;

        // Prepare the SQL query for inserting a new reply
        string insertReplyQuery = @"
    INSERT INTO Reply_Comment (Id, comment_id, Date, Comment) 
    VALUES (@Id, @comment_id, @Date, @Comment);
";

        // Execute the query to insert the new reply
        var result = DataAccess.DataAccess.ExecuteNonQuery(insertReplyQuery, new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@comment_id", model.CommentId }, // Assuming this matches the property in your ReplyCommentModel
        { "@Date", model.Date },
        { "@Comment", model.Comment } // Make sure this matches the property in your ReplyCommentModel
    });

        // Optionally, you could check the result for success/failure
    }

    public void DeleteComment(string commentId)
    {
        // Prepare the SQL query for deleting a comment
        var query = "DELETE FROM Comment WHERE Id = @Id";
        var parameter = new Dictionary<string, object>
    {
        { "@Id", commentId }
    };

        // Execute the query
        DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
    }

    public void DeleteReply(string replyId)
    {
        // Prepare the SQL query for deleting a reply
        var query = "DELETE FROM Reply_Comment WHERE Id = @Id";
        var parameter = new Dictionary<string, object>
    {
        { "@Id", replyId }
    };

        // Execute the query
        DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
    }

    public void DeleteAllReply(string commentId)
    {
        // Prepare the SQL query for deleting all replies related to a specific comment
        var query = "DELETE FROM Reply_Comment WHERE comment_id = @comment_id";
        var parameter = new Dictionary<string, object>
    {
        { "@comment_id", commentId }
    };

        // Execute the query
        DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
    }



    public void UpdateComment(string id, string comment)
    {
        string querry = "Update Comment SET Comment=@Comment Where Id=@Id";
        var row = DataAccess.DataAccess.ExecuteNonQuery(querry, new Dictionary<string, object>
        {
            { "@Comment",comment},
            {"@Id",id }
        });
          
      
    }
    public void UpdateReply(string id, string comment)
    {
          string querry = "Update Reply_Comment SET Comment=@Comment Where Id=@Id";
        var row = DataAccess.DataAccess.ExecuteNonQuery(querry, new Dictionary<string, object>
        {
            { "@Comment",comment},
            {"@Id",id }
        });
    }
    public List<CommentModel> GetCommentsByProductId(string productId)
    {
        // SQL query to retrieve comments along with user and reply details
        string query = @"
       SELECT 
            c.Id AS CommentId, 
            c.Comment AS Comment, 
            c.Date AS CommentDate, 
            a.Username AS UserName, 
            u.Name AS FullName,
            rc.Id AS ReplyId, 
            rc.comment_id AS ReplyCommentId, 
            rc.Comment AS ReplyComment, 
            rc.Date AS Date
        FROM 
            Comment c
        JOIN 
            Account a ON c.user_id = a.Id
        JOIN 
            User u ON c.user_id = u.account_id
        LEFT JOIN 
            Reply_Comment rc ON rc.comment_id = c.Id
        WHERE 
            c.product_id = @ProductId
        ORDER BY 
            c.Date DESC, rc.Date ASC;";

        // Execute the query and get results into a DataTable
        var parameters = new Dictionary<string, object>
    {
        { "@ProductId", productId }
    };
        DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

        // Parse the results into a list of CommentModel objects with nested replies
        var comments = new List<CommentModel>();
        var repliesDict = new Dictionary<string, List<ReplyCommentModel>>();

        foreach (DataRow row in resultTable.Rows)
        {
            // Retrieve comment details
            string commentId = row["CommentId"].ToString();
            if (!comments.Any(c => c.Id == commentId))
            {
                comments.Add(new CommentModel
                {
                    Id = commentId,
                    Comment = row["Comment"].ToString(),
                    Date = Convert.ToDateTime(row["CommentDate"]),
                    UserName = row["UserName"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Replies = new List<ReplyCommentModel>()
                });
            }

            // Retrieve reply details if present
            if (!row.IsNull("ReplyId"))
            {
                var reply = new ReplyCommentModel
                {
                    Id = row["ReplyId"].ToString(),
                    CommentId = row["ReplyCommentId"].ToString(),
                    Comment = row["ReplyComment"].ToString(),
                    Date = Convert.ToDateTime(row["Date"])
                };

                // Add reply to the comment's reply list
                var parentComment = comments.FirstOrDefault(c => c.Id == commentId);
                parentComment?.Replies.Add(reply);
            }
        }

        return comments;
    }

    public List<CommentModel> GetAllComments()
    {
        // SQL query to retrieve all comments with user, product, category, and reply details
        string query = @"
        SELECT 
            c.Id AS CommentId, 
            c.Comment AS CommentText, 
            c.Date AS CommentDate, 
            a.Username AS UserName, 
            u.Name AS FullName, 
            p.Id AS ProductId,
            p.Name AS ProductName,
            p.Picture AS ProductPicture,
            p.Description AS ProductDescription,
            cat.Id AS CategoryId,
            cat.Name AS CategoryName,
            rc.Id AS ReplyId,
            rc.comment_id AS ReplyCommentId,
            rc.Comment AS ReplyText,
            rc.Date AS ReplyDate
        FROM 
            Comment c
        JOIN 
            Account a ON c.user_id = a.Id
        JOIN 
            User u ON c.user_id = u.account_id
        JOIN 
            Product p ON c.product_id = p.Id
        JOIN 
            Category cat ON p.category_id = cat.Id
        LEFT JOIN 
            Reply_Comment rc ON rc.comment_id = c.Id
        ORDER BY 
            c.Date DESC, rc.Date ASC;";

        // Execute the query and get results into a DataTable
        DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

        // Parse the results into a list of CommentModel objects with nested replies
        var comments = new List<CommentModel>();
        var repliesDict = new Dictionary<string, List<ReplyCommentModel>>();

        foreach (DataRow row in resultTable.Rows)
        {
            // Retrieve comment details
            string commentId = row["CommentId"].ToString();
            if (!comments.Any(c => c.Id == commentId))
            {
                comments.Add(new CommentModel
                {
                    Id = commentId,
                    Comment = row["CommentText"].ToString(),
                    Date = Convert.ToDateTime(row["CommentDate"]),
                    UserName = row["UserName"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Product = new ProductModel
                    {
                        Id = row["ProductId"].ToString(),
                        Name = row["ProductName"].ToString(),
                        Picture = row["ProductPicture"].ToString(),
                        Description = row["ProductDescription"].ToString(),
                        CategoryId = row["CategoryId"].ToString(),
                        CategoryName = row["CategoryName"].ToString(),
                    },
                    Replies = new List<ReplyCommentModel>()
                });
            }

            // Retrieve reply details if present
            if (!row.IsNull("ReplyId"))
            {
                var reply = new ReplyCommentModel
                {
                    Id = row["ReplyId"].ToString(),
                    CommentId = row["ReplyCommentId"].ToString(),
                    Comment = row["ReplyText"].ToString(),
                    Date = Convert.ToDateTime(row["ReplyDate"])
                };

                // Add reply to the comment's reply list
                var parentComment = comments.FirstOrDefault(c => c.Id == commentId);
                parentComment?.Replies.Add(reply);
            }
        }

        return comments;
    }

}

