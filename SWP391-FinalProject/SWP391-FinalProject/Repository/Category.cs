using SWP391_FinalProject.Entities;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;  // Ensure you have this namespace

namespace SWP391_FinalProject.Repository
{
    public class Category
    {
        private readonly DBContext db;

        public Category(DBContext context)
        {
            db = context;
        }
        public List<Models.CategoryModel> GetAllCategory()
        {
            var list = db.Categories.Select(p => new Models.CategoryModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();  // Convert IQueryable to List

            return list;
        }
        public List<Models.CategoryModel> GetAllCatLaps()
        {
            var category = db.Categories.AsQueryable();
            List<Models.CategoryModel> result = category
            .Where(c => c.Id.StartsWith("B0"))
               .Select(c => new Models.CategoryModel
               {
                   Id = c.Id,
                   Name = c.Name                  
               })
               .ToList(); // Materialize the query
            return result;
        }
        public List<Models.CategoryModel> GetAllCatPhone()
        {
            var category = db.Categories.AsQueryable();
            List<Models.CategoryModel> result = category
            .Where(c => c.Id.StartsWith("B1"))
               .Select(c => new Models.CategoryModel
               {
                   Id = c.Id,
                   Name = c.Name
               })
               .ToList(); // Materialize the query
            return result;
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
            var categoryEntity= db.Categories.FirstOrDefault(c => c.Id == id);
            if (categoryEntity == null)
            {
                return null; // Or handle not found scenario appropriately
            }
            Models.CategoryModel result = new Models.CategoryModel
            {
                Id=categoryEntity.Id,
                Name = categoryEntity.Name,
            };
            return (result);
            
        }
    }
}

