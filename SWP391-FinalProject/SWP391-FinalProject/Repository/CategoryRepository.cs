using SWP391_FinalProject.Entities;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using SWP391_FinalProject.Models;
using Microsoft.EntityFrameworkCore;  // Ensure you have this namespace
using SWP391_FinalProject.DataAccess;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class CategoryRepository
    {
        private readonly DBContext db;

        public CategoryRepository()
        {
            db = new DBContext();
        }
        public bool DeleteCategory(string categoryId)
        {
            var query = "DELETE FROM Category WHERE Id = @Id";
            var parameter = new Dictionary<string, object>
            {
                { "@Id", categoryId }
            };

            int row = DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
            if(row == 0)
            {
                return false;
            }
            return true;
        }
        public List<Models.CategoryModel> GetAllCategory()
        {
            // Define the query to retrieve all categories
            string query = "SELECT Id, Name FROM Category";

            // Retrieve data from the database using the DataAccessLayer
            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Map the DataTable rows to a list of CategoryModel
            var categories = new List<Models.CategoryModel>();
            foreach (DataRow row in categoryTable.Rows)
            {
                categories.Add(new Models.CategoryModel
                {
                    Id = Convert.ToString(row["Id"]),
                    Name = row["Name"].ToString()
                });
            }

            return categories;
        }
        public List<Models.CategoryModel> GetAllCatLaps()
        {
            var categories = new List<Models.CategoryModel>();

            string query = "SELECT * FROM Category WHERE Id LIKE 'B0%';";

            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(query);

            foreach (DataRow row in categoryTable.Rows)
            {
                categories.Add(new CategoryModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString()
                });
            }

            return categories;
        }
        public List<Models.CategoryModel> GetAllCatPhone()
        {
            var categories = new List<Models.CategoryModel>();

            string query = "SELECT * FROM Category WHERE Id LIKE 'B1%';";

            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(query);

            foreach (DataRow row in categoryTable.Rows)
            {
                categories.Add(new CategoryModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString()
                });
            }
            return categories;
        }
        public string GenerateCategoryId(string prefix)
        {
            // Dynamic query to get the last ID with the specified prefix
            string query = $"SELECT Id FROM Category WHERE Id LIKE '{prefix}%' ORDER BY Id DESC LIMIT 1;";
            DataTable existingCategories = DataAccess.DataAccess.ExecuteQuery(query);

            int nextIdNumber = 1; // Default to 1 if no existing ID

            if (existingCategories != null && existingCategories.Rows.Count > 0)
            {
                // Retrieve the last ID from the result set
                string lastId = existingCategories.Rows[0]["Id"].ToString();

                // Extract the numeric part and increment
                nextIdNumber = int.Parse(lastId.Substring(prefix.Length)) + 1;
            }
            else
            {
                return "B0000001";
            }

            // Generate new ID with the specified format
            return $"{prefix}{nextIdNumber.ToString("D6")}";
        }

        public Models.CategoryModel GetCatById(string id)
        {
            string query = "SELECT * FROM Category WHERE id = @id";
            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };
            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            DataRow row = categoryTable.Rows[0];
            Models.CategoryModel result = new Models.CategoryModel
            {
                Id = row["Id"].ToString(),
                Name = row["Name"].ToString(),
            };

            return result;

        }

        public void AddCategory(string Name, string CategoryType)
        {
            string categoryId = "";
            // Determine category ID based on selected CategoryType
            if (CategoryType == "laptop")
            {
                categoryId = GenerateCategoryId("B0");
            }
            else if (CategoryType == "phone")
            {
                categoryId = GenerateCategoryId("B1");
            }

            string query = "INSERT INTO Category(Id, Name) VALUES(@categoryId, @Name)";
            var count = DataAccess.DataAccess.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@categoryId", categoryId },
                { "@Name", Name }
            });
        }

        public void EditCategory(CategoryModel category)
        {
            // Define the SQL query to update the category name
            string query = "UPDATE Category SET Name = @Name WHERE Id = @Id";

            // Call ExecuteNonQuery in DataAccessLayer and pass the query with parameters
            var row = DataAccess.DataAccess.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@Name", category.Name },
                { "@Id", category.Id }
            });
        }

    }
}


