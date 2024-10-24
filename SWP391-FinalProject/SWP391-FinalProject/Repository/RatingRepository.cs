using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
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
         public double GetAverageRating(string productId)
        {
            var averageRating = db.Ratings
             .Where(r => r.ProductId == productId)
             .Select(r => (double?)r.Rating1)  // Chuyển Rating1 thành kiểu nullable double
             .AsEnumerable()                   // Chuyển về phía client để xử lý
             .DefaultIfEmpty(null)             // Nếu không có giá trị, trả về null
             .Average();

            return averageRating??5;
        }

        public void InsertOrUpdateRating(RatingModel rating)
        {
            UserRepository UserRepo = new UserRepository();
            UserModel user = UserRepo.GetUserProfileByUsername(rating.Username);

            // Tìm rating hiện có của user cho cùng ProductId
            Rating existingRating = db.Ratings
                .FirstOrDefault(r => r.ProductId == rating.ProductId && r.UserId == user.Account.Id);

            if (existingRating != null)
            {
                // Nếu đã tồn tại, cập nhật Rating1
                existingRating.Rating1 = rating.Rating;
                db.SaveChanges();
            }
            else
            {
                // Nếu không tồn tại, thêm mới
                Rating newRating = new Rating
                {
                    ProductId = rating.ProductId,
                    Rating1 = rating.Rating,
                    UserId = user.Account.Id,
                    Id = NewRatingId()
                };

                db.Ratings.Add(newRating);
                db.SaveChanges();
            }
        }
        public void UpdateRating(RatingModel rating)
        {
            Rating existingRating = db.Ratings
                .FirstOrDefault(r => r.Id == rating.Id);

            if (existingRating != null)
            {
                existingRating.Rating1 = rating.Rating;
                db.SaveChanges();
            }
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
