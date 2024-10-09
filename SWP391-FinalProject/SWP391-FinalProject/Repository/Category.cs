using SWP391_FinalProject.Entities;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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

    }
}
