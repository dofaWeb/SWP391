using Microsoft.CodeAnalysis;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;

namespace SWP391_FinalProject.Repository
{
    public class ProductRepository
    {
        private readonly DBContext db;

        public ProductRepository()
        {
            db  = new DBContext();
        }

        public List<Models.ProductModel> GetProductsByKeyword(string keyword)
        {
            // Check for null or empty keyword and return an empty list if so
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Models.ProductModel>();
            }

            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        where p.Name.Contains(keyword) || p.Name.StartsWith(keyword)
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
        public List<Models.ProductModel> GetProductByBrand(string brand)
        {
            // Check for null or empty keyword and return an empty list if so
            if (string.IsNullOrWhiteSpace(brand))
            {
                return new List<Models.ProductModel>();
            }

            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        where c.Name.Contains(brand)
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

        public List<Models.ProductModel> ProductsByCategory(string type)
        {
            // Check for null or empty type and return an empty list if so
            if (string.IsNullOrWhiteSpace(type))
            {
                return new List<Models.ProductModel>();
            }

            string keyword = "";
            switch (type)
            {
                case "laptops":
                    keyword = "B0";
                    break;
                case "phones":
                    keyword = "B1";
                    break;
                    // Add more cases as needed
            }

            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        where p.CategoryId.StartsWith(keyword)
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

        public string GetProductVariationOption(string productItemId, string option)
        {
            var varianceValue = (from pc in db.ProductConfigurations
                                 join vo in db.VariationOptions on pc.VariationOptionId equals vo.Id
                                 join va in db.Variations on vo.VariationId equals va.Id
                                 where pc.ProductItemId == productItemId && va.Name == option
                                 select vo.Value).FirstOrDefault();

            return varianceValue; // Return empty string if result is null
        }

        public decimal CalculatePriceAfterDiscount(decimal? SellingPrice, decimal? discount)
        {
            var d = (discount == null) ? 0 : discount;
            var s = (SellingPrice == null) ? 0 : SellingPrice;
            return (decimal)(s - (s * d));
        }

        public decimal? CalculateProfit(decimal? PriceAfterDiscount, decimal ImportPrice)
        {
            return PriceAfterDiscount - ImportPrice;
        }

        public List<ProductItemModel> GetProductItem(string productId)
        {
            var productItems = db.ProductItems.Where(p => p.ProductId == productId).ToList();
            var result = productItems.Select(p => new ProductItemModel
            {
                Id = p.Id,
                Quantity = p.Quantity,
                ImportPrice = p.ImportPrice,
                SellingPrice = p.SellingPrice,
                Ram = GetProductVariationOption(p.Id,"Ram"),
                Storage = GetProductVariationOption(p.Id, "Storage"),
                Discount = p.Discount,
                PriceAfterDiscount = CalculatePriceAfterDiscount(p.SellingPrice, p.Discount/100),
                Profit = CalculateProfit(CalculatePriceAfterDiscount(p.SellingPrice, p.Discount/100),p.ImportPrice),
                ProductId = productId
            }).ToList();

            return result;
        }
        public ProductItemModel GetProductItemById(string productId)
        {
            // Truy vấn dữ liệu bằng LINQ
            var productItem = (from p in db.Products
                               join pi in db.ProductItems on p.Id equals pi.ProductId
                              // Nếu có bảng Variations
                               where p.Id == productId
                               select new ProductItemModel
                               {
                                   Id = pi.Id,
                                   ProductId = pi.ProductId,
                                   Product = new ProductModel
                                   {
                                       Id = p.Id,
                                       Name = p.Name,
                                       Picture = p.Picture,
                                       Description = p.Description,
                                       CategoryId = p.CategoryId,
                                       CategoryName = db.Categories
                                                   .Where(c => c.Id == p.CategoryId)
                                                   .Select(c => c.Name).FirstOrDefault(), // Lấy tên danh mục

                                       StateId = p.StateId
                                   },
                                   Quantity = pi.Quantity,
                                   ImportPrice = pi.ImportPrice,
                                   SellingPrice = pi.SellingPrice,
                                   Discount = pi.Discount,

                               }).FirstOrDefault();

            return productItem;

        }

        //public ProductItemModel GetProductItemById(string productItemId)
        //{
        //    var productItems = from pi in db.ProductItems 
        //                       join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId


        //    return productItems;
        //}

    }
}
