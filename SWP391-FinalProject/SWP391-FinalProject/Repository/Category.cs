using SWP391_FinalProject.Entities;

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

    }
}
