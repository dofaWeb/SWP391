using Microsoft.CodeAnalysis;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class ProductRepository
    {
        private readonly DBContext db;

        public ProductRepository(DBContext context)
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

        public List<Models.ProductModel> GetAllProduct()
        {
            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        select new Models.ProductModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Picture = p.Picture,
                            Quantity = (db.ProductItems
                                        .Where(pi => pi.ProductId == p.Id)
                                        .Sum(pi => (int?)pi.Quantity) ?? 0), // Handling NULL by converting to 0
                            CategoryName = c.Name,
                            ProductState = ps.Name
                        };

            var result = query.ToList(); // Execute the query
            return result;
        }

        public void UpdateProductState(string productId)
        {
            // Calculate the total quantity for the product
            var totalQuantity = db.ProductItems
                .Where(pi => pi.ProductId == productId)
                .Sum(pi => (int?)pi.Quantity) ?? 0; // Use nullable int and default to 0

            // Check if the total quantity is 0
            if (totalQuantity == 0)
            {
                // Find the product to update
                var productToUpdate = db.Products.FirstOrDefault(p => p.Id == productId);

                // Update the state to 3 if the product exists
                if (productToUpdate != null)
                {
                    productToUpdate.State.Id = 3; // Set state to 3

                    db.SaveChanges(); // Save changes to the database
                }
            }
        }

        public string getNewProductID()
        {
            string lastId = db.Products
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "P0000001";
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

        public List<Models.ProductStateModel> getAllProductState()
        {
            var list = db.ProductStates.Select(p => new Models.ProductStateModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
            return list;
        }

        public void AddProduct(Models.ProductModel model, IFormFile pictureUpload)
        {
            var newProduct = new Entities.Product
            {
                Id = model.Id,
                Name = model.Name,
                CategoryId = model.CategoryId,
                StateId = 3,
                Description = model.Description,
            };
            if (pictureUpload != null)
            {
                newProduct.Picture = MyUtil.UpLoadPicture(pictureUpload);
            }
            db.Products.Add(newProduct);
            db.SaveChanges();
        }

        public int? GetProductQuantityById(string id)
        {
            var totalQuantity = (from p in db.Products
                                 join pi in db.ProductItems on p.Id equals pi.ProductId
                                 where p.Id == id
                                 select pi.Quantity).Sum();
            return totalQuantity;
        }

        public void UpdateProduct(ProductModel model, IFormFile pictureUpload)
        {
            // Fetch the existing product from the database
            var existingProduct = db.Products.FirstOrDefault(p => p.Id == model.Id);
            if (existingProduct == null)
                throw new Exception("Product not found!");

            // Update the fields that are allowed to change
            existingProduct.Name = model.Name;
            existingProduct.CategoryId = model.CategoryId;
            existingProduct.Description = model.Description;
            if (GetProductQuantityById(model.Id) > 0)
            {
                existingProduct.StateId = model.StateId;
            }
            else
                existingProduct.StateId = 3;//out of stock

            // Handle picture upload
            if (pictureUpload != null)
            {
                MyUtil.DeletePicture(existingProduct.Picture);  // Delete old picture
                existingProduct.Picture = MyUtil.UpLoadPicture(pictureUpload);  // Upload new one
            }

            // Save changes to the database
            using var transaction = db.Database.BeginTransaction();
            try
            {
                db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        public Models.ProductModel GetProductById(string id)
        {
            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        where p.Id == id
                        select new Models.ProductModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Picture = p.Picture,
                            Quantity = (db.ProductItems
                                        .Where(pi => pi.ProductId == p.Id)
                                        .Sum(pi => (int?)pi.Quantity) ?? 0), // Handling NULL by converting to 0
                            CategoryName = c.Name,
                            ProductState = ps.Name,
                            CategoryId = c.Id
                        };
            var product = query.FirstOrDefault();
            return product;
        }
    }
}
