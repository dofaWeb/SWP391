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
        public List<Models.ProductModel> GetProductsByKeyword(string keyword)
        {
            // Check for null or empty keyword and return an empty list if so
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Models.ProductModel>();
            }

            var products = db.Products.AsQueryable();

            // Use 'Contains' for 'like' behavior (e.g., '%keyword%') or 'StartsWith' for 'starts with' behavior
            List<Models.ProductModel> result = products
                .Where(p => p.Name.Contains(keyword) || p.Name.StartsWith(keyword))
                .Select(p => new Models.ProductModel
                {
                    Name = p.Name,
                    Picture = p.Picture,
                    CategoryId = p.CategoryId,
                    Description = p.Description
                })
                .ToList(); // Materialize the query

            return result;
        }
    }
}
