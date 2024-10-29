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
            var category = db.Categories.Find(categoryId);

            if (category == null)
            {
                return false; // Return false if category is not found
            }

            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();
                return true; // Return true if deletion is successful
            }
            catch (DbUpdateException)
            {
                // Foreign key constraint violation or other issue
                return false; // Return false if unable to delete
            }
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

            foreach(DataRow row in categoryTable.Rows)
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
            // Get the number of existing categories with the same prefix to generate unique ID
            var existingCategories = db.Categories
                                       .Where(c => c.Id.StartsWith(prefix))
                                       .OrderByDescending(c => c.Id)
                                       .FirstOrDefault();

            int nextIdNumber = 1; // Default to 1 if there are no existing IDs

            if (existingCategories != null)
            {
                // Extract the numeric part from the existing category ID
                string lastId = existingCategories.Id;
                nextIdNumber = int.Parse(lastId.Substring(2)) + 1;
            }

            // Generate new ID with format
            return $"{prefix}{nextIdNumber.ToString("D6")}";
        }
        public Models.CategoryModel GetCatById(string id)
        {
            var categoryEntity = db.Categories.FirstOrDefault(c => c.Id == id);
            if (categoryEntity == null)
            {
                return null; // Or handle not found scenario appropriately
            }
            Models.CategoryModel result = new Models.CategoryModel
            {
                Id = categoryEntity.Id,
                Name = categoryEntity.Name,
            };
            return (result);

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


