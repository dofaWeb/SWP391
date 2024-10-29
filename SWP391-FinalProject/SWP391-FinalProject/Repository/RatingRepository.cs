using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class RatingRepository
    {
        private readonly DBContext db;

        public RatingRepository()
        {
            db = new DBContext();
        }

        public double GetAverageRating(string productId)
        {
            string query = "SELECT AVG(CAST(rating AS FLOAT)) AS AverageRating " +  // Sử dụng 'rating' thay cho các tên cột khác
                           "FROM Rating " +
                           "WHERE product_id = @ProductId";

            var parameters = new Dictionary<string, object>
            {
                { "@ProductId", productId }
            };

            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            if (resultTable.Rows.Count > 0 && resultTable.Rows[0]["AverageRating"] != DBNull.Value)
            {
                return Convert.ToDouble(resultTable.Rows[0]["AverageRating"]);
            }

            return 5.0; // Mặc định trả về 5 nếu không có rating
        }

        public void InsertOrUpdateRating(RatingModel ratingModel)
        {
            // Kiểm tra xem user_id có hợp lệ không
            if (string.IsNullOrEmpty(ratingModel.UserId))
            {
                throw new ArgumentException("UserId cannot be null or empty.");
            }

            string checkQuery = "SELECT * FROM Rating WHERE product_id = @ProductId AND user_id = @UserId";
            var checkParameters = new Dictionary<string, object>
    {
        { "@ProductId", ratingModel.ProductId },
        { "@UserId", ratingModel.UserId }
    };

            DataTable existingRatingTable = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (existingRatingTable.Rows.Count > 0)
            {
                // Cập nhật rating nếu đã tồn tại
                string updateQuery = "UPDATE Rating SET rating = @Rating WHERE product_id = @ProductId AND user_id = @UserId";
                var updateParameters = new Dictionary<string, object>
        {
            { "@Rating", ratingModel.Rating },
            { "@ProductId", ratingModel.ProductId },
            { "@UserId", ratingModel.UserId }
        };
                DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
            }
            else
            {
                // Thêm mới rating nếu chưa tồn tại
                string insertQuery = "INSERT INTO Rating (id, product_id, user_id, rating) VALUES (@Id, @ProductId, @UserId, @Rating)";
                var insertParameters = new Dictionary<string, object>
        {
            { "@Id", NewRatingId() },
            { "@ProductId", ratingModel.ProductId },
            { "@UserId", ratingModel.UserId },
            { "@Rating", ratingModel.Rating }
        };
                DataAccess.DataAccess.ExecuteNonQuery(insertQuery, insertParameters);
            }
        }


        public void UpdateRating(RatingModel ratingModel)
        {
            string query = "UPDATE Rating SET rating = @Rating WHERE id = @Id";  // Sử dụng 'rating'
            var parameters = new Dictionary<string, object>
            {
                { "@Rating", ratingModel.Rating },
                { "@Id", ratingModel.Id }
            };
            DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }

        public string NewRatingId()
        {
            string query = "SELECT id FROM Rating ORDER BY id DESC LIMIT 1";
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

            if (resultTable.Rows.Count == 0)
            {
                return "R0000001"; // ID đầu tiên nếu không có bản ghi nào
            }

            string lastId = resultTable.Rows[0]["id"].ToString();
            string prefix = lastId.Substring(0, 1);
            int number = int.Parse(lastId.Substring(1));

            int newNumber = number + 1;
            string newId = $"{prefix}{newNumber:D7}"; // Đảm bảo giữ đúng định dạng với 7 chữ số

            return newId;
        }

    }
}
