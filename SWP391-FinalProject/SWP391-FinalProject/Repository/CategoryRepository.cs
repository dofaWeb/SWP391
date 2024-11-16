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


        public CategoryRepository()
        {

        }
        public bool DeleteCategory(string categoryId)
        {
            var parameter = new Dictionary<string, object>
            {
                { "@Id", categoryId }
            };

            var check = "SELECT * FROM Category c JOIN Product p ON c.id = p.category_id Where c.id = @Id";

            var result = DataAccess.DataAccess.ExecuteQuery(check, parameter);

            if (result.Rows.Count == 0)
            {

                var query = "DELETE FROM Category WHERE Id = @Id";


                int row = DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
                if (row == 0)
                {
                    return false;
                }
                return true;
            }
            return false;
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

        public bool AddCategory(string Name, string CategoryType)
        {
            var type = (CategoryType == "laptop") ? "B0" : "B1";
            string check = "SELECT * FROM Category Where Name = @Name And Id Like '" + type + "%'" ;
            var parameter = new Dictionary<string, object> {
                {"@Name", Name }
            };
            var result = DataAccess.DataAccess.ExecuteQuery(check, parameter);
            if (result.Rows.Count == 0)
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
                return true;
            }
            return false;
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
        public List<Models.CategoryModel> GetAllCategoryByKeyword(string keyword)
        {
            // SQL query to retrieve all categories matching the keyword
            string categoryQuery = @"
        SELECT Id AS CategoryId, 
               Name AS CategoryName
        FROM Category
        WHERE Name LIKE @keyword OR Id LIKE @keyword";

            // Define the parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@keyword", $"%{keyword}%" }
    };

            // Execute the query to get category details
            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(categoryQuery, parameters);
            var categories = new List<Models.CategoryModel>();

            foreach (DataRow row in categoryTable.Rows)
            {
                var categoryModel = new Models.CategoryModel
                {
                    Id = row["CategoryId"].ToString(),
                    Name = row["CategoryName"].ToString()
                };

                // Add the category model to the list
                categories.Add(categoryModel);
            }

            return categories;
        }


    }
}


