using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Security.Cryptography.X509Certificates;

namespace SWP391_FinalProject.Repository
{
    public class RatingRepository
    {
        DBContext db;
        public RatingRepository()
        {
            db = new DBContext();
        }
        public void InsertRating(RatingModel rating) 
        { 
            Rating newRating = new Rating();
            newRating.ProductId = rating.ProductId;
            newRating.Rating1 = rating.Rating;
            UserRepository UserRepo = new UserRepository();
            UserModel user = UserRepo.GetUserProfileByUsername(rating.Username);
            newRating.UserId = user.Account.Id;
            newRating.Id = NewRatingId();
            db.Ratings.Add(newRating);
            db.SaveChanges();
        }
        public string NewRatingId()
        {
            string lastId = db.Ratings
                .OrderByDescending(r => r.Id)
                .Select(r => r.Id)
                .FirstOrDefault();
            if (lastId == null)
            {
                return "R0000001";
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
        
    }
}
