using SWP391_FinalProject.Entities;

namespace SWP391_FinalProject.Repository
{
    public class Product
    {
        private readonly DBContext db;

        public Product(DBContext context)
        {
            db = context;
        }
        public List<Models.Product> GetProductsByKeyword(string keyword)
        {
            // Check for null or empty keyword and return an empty list if so
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Models.Product>();
            }

            var products = db.Products.AsQueryable();

            // Use 'Contains' for 'like' behavior (e.g., '%keyword%') or 'StartsWith' for 'starts with' behavior
            List<Models.Product> result = products
                .Where(p => p.Name.Contains(keyword) || p.Name.StartsWith(keyword))
                .Select(p => new Models.Product
                {
                    Name = p.Name,
                    Picture = p.Picture,
                    Category = p.CategoryId,
                    Description = p.Description
                })
                .ToList(); // Materialize the query

            return result;
        }
    }
}
