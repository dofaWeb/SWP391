using K4os.Compression.LZ4.Streams.Adapters;
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
            db = new DBContext();
        }

        public List<Models.ProductModel> GetProductsByKeyword(string keyword, string price, string category)
        {
            // Start querying products without keyword filtering
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
                            CategoryId = c.Id,
                            ProductState = ps.Name
                        };

            // Check for null or empty keyword and apply filtering only if it's present
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || p.Name.StartsWith(keyword));
            }

            // Apply category filtering if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category.Equals("Laptop", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.CategoryId.StartsWith("B0"));
                }
                else if (category.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.CategoryId.StartsWith("B1"));
                }
            }

            // Execute the query and retrieve the results
            var result = query.ToList(); // Execute the query

            // Calculate minimum price for each product item
            foreach (var item in result)
            {
                var minPriceProductItem = db.ProductItems
                    .Where(pi => pi.ProductId == item.Id && pi.SellingPrice.HasValue)
                    .Select(pi => new ProductItemModel
                    {
                        Discount = pi.Discount,
                        Id = pi.Id,
                        SellingPrice = pi.SellingPrice,
                        PriceAfterDiscount = CalculatePriceAfterDiscount(pi.SellingPrice, pi.Discount / 100),
                        Saving = pi.SellingPrice - CalculatePriceAfterDiscount(pi.SellingPrice, pi.Discount / 100)
                    })
                    .OrderBy(pi => pi.SellingPrice)
                    .FirstOrDefault(); // Take the minimum priced product item

                item.ProductItem = minPriceProductItem; // Set the minimum price item to ProductModel
            }

            // Apply price sorting if provided
            if (!string.IsNullOrWhiteSpace(price))
            {
                if (price.Equals("Asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(p => p.ProductItem.PriceAfterDiscount).ToList(); // Sort by minimum price ascending
                }
                else if (price.Equals("Desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(p => p.ProductItem.PriceAfterDiscount).ToList(); // Sort by minimum price descending
                }
            }

            return result; // Return the final list of products
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
            foreach (var item in result)
            {
                item.ProductItem = GetMinPrice(item.Id);

            }
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
            foreach (var item in result)
            {
                item.ProductItem = GetMinPrice(item.Id);

            }
            return result;
        }

        public decimal? GetPrice(string ram, string storage, string productId)
        {

            var model = (from p in db.Products
                         join pi in db.ProductItems on p.Id equals pi.ProductId
                         join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId
                         join vo in db.VariationOptions on pc.VariationOptionId equals vo.Id
                         join va in db.Variations on vo.VariationId equals va.Id
                         where vo.Value == ram
                               && p.Id == productId
                               && (from p2 in db.Products
                                   join pi2 in db.ProductItems on p2.Id equals pi2.ProductId
                                   join pc2 in db.ProductConfigurations on pi2.Id equals pc2.ProductItemId
                                   join vo2 in db.VariationOptions on pc2.VariationOptionId equals vo2.Id
                                   join va2 in db.Variations on vo2.VariationId equals va2.Id
                                   where vo2.Value == storage && p2.Id == productId
                                   select pi2.Id).Contains(pi.Id)  // IN equivalent
                         select new
                         {
                             SellingPrice = pi.SellingPrice,
                             Discount = pi.Discount
                         }).FirstOrDefault();



            return ProductRepository.CalculatePriceAfterDiscount(model.SellingPrice, model.Discount / 100);
        }

        public string GetProItemIdByVariation(string ram, string storage, string productId)
        {
            var ProItemId = (from p in db.Products
                         join pi in db.ProductItems on p.Id equals pi.ProductId
                         join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId
                         join vo in db.VariationOptions on pc.VariationOptionId equals vo.Id
                         join va in db.Variations on vo.VariationId equals va.Id
                         where vo.Value == ram
                               && p.Id == productId
                               && (from p2 in db.Products
                                   join pi2 in db.ProductItems on p2.Id equals pi2.ProductId
                                   join pc2 in db.ProductConfigurations on pi2.Id equals pc2.ProductItemId
                                   join vo2 in db.VariationOptions on pc2.VariationOptionId equals vo2.Id
                                   join va2 in db.Variations on vo2.VariationId equals va2.Id
                                   where vo2.Value == storage && p2.Id == productId
                                   select pi2.Id).Contains(pi.Id)  // IN equivalent
                         select pi.Id).FirstOrDefault();

            return ProItemId;
        }

        public ProductItemModel GetMinPrice(string productId)
        {
            var minPriceProductItem = (from p in db.Products
                                       join pi in db.ProductItems on p.Id equals pi.ProductId
                                       where pi.SellingPrice.HasValue && p.Id == productId
                                       select new ProductItemModel
                                       {
                                           Discount = pi.Discount,
                                           Id = pi.Id,
                                           SellingPrice = pi.SellingPrice,
                                           PriceAfterDiscount = CalculatePriceAfterDiscount(pi.SellingPrice, pi.Discount / 100),
                                           Saving = pi.SellingPrice - CalculatePriceAfterDiscount(pi.SellingPrice, pi.Discount / 100)

                                       })
                                       .OrderBy(pi => pi.SellingPrice)  // Order by SellingPrice in ascending order
                                       .FirstOrDefault();               // Take the first item (i.e., the minimum price)

            return minPriceProductItem;
        }

        public List<Models.ProductModel> GetTopSellingProduct()
        {
            // Get top 4 best-selling products where the order state is approved (state_id = 2)
            var topProducts = (from p in db.Products
                               join pi in db.ProductItems on p.Id equals pi.ProductId
                               join oi in db.OrderItems on pi.Id equals oi.ProductItemId
                               join o in db.Orders on oi.OrderId equals o.Id
                               where o.StateId == 2 && p.StateId  == 1
                               group p by new { p.Id, p.Name } into g
                               select new
                               {
                                   ProductId = g.Key.Id,
                                   ProductName = g.Key.Name,
                                   TotalPurchases = g.Count()
                               })
                              .OrderByDescending(x => x.TotalPurchases)
                              .Take(4)
                              .ToList();

            // Query to get all products and related data (category, state, quantity)
            var query = from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.Id
                        join ps in db.ProductStates on p.StateId equals ps.Id
                        where topProducts.Select(tp => tp.ProductId).Contains(p.Id) // Only include top products
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

            // Execute the query
            var result = query.ToList();

            // Get minimum price for each product
            foreach (var item in result)
            {
                item.ProductItem = GetMinPrice(item.Id); // Assuming this method gets the minimum price
            }

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
                            ProductState = ps.Name,


                        };

            var result = query.ToList(); // Execute the query
            foreach (var item in result)
            {
                item.ProductItem = GetMinPrice(item.Id);

            }
            return result;
        }

        public void UpdateProductState(string productId)
        {
            // Calculate the total quantity for the product
            var totalQuantity = db.ProductItems
                .Where(pi => pi.ProductId == productId)
                .Sum(pi => (int?)pi.Quantity) ?? 0; // Use nullable int and default to 0

            // Find the product to update
            var productToUpdate = db.Products.FirstOrDefault(p => p.Id == productId);

            if (productToUpdate != null)
            {
                // Check if the total quantity is 0
                if (totalQuantity == 0)
                {
                    // Update the state to 3 if the product exists

                    productToUpdate.StateId = 3; // Set state to 
                }
                else
                {
                    productToUpdate.StateId = 1;
                }
                db.SaveChanges(); // Save changes to the database
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

        public List<string> GetProductItemIdByProductId(string productId)
        {
            var proItemId = from pi in db.ProductItems
                            where pi.ProductId == productId
                            select pi.Id;
            var result = proItemId.ToList();

            return result; // This now matches List<string>
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

        public static decimal CalculatePriceAfterDiscount(decimal? SellingPrice, decimal? discount)
        {
            try
            {
                if (SellingPrice < 0)
                    throw new ArgumentException("Selling price cannot be negative.");
                if (discount < 0|| discount > 1)
                {
                    throw new ArgumentException("Discount must be between 0-100.");
                }
                // Set discount to 0 if it's null, and similarly for SellingPrice
                var d = discount ?? 0;
                var s = SellingPrice ?? 0;

                return s - (s * d);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                throw; // Rethrow to allow the calling code or tests to handle it
            }
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
                Ram = GetProductVariationOption(p.Id, "Ram"),
                Storage = GetProductVariationOption(p.Id, "Storage"),
                Discount = p.Discount,
                PriceAfterDiscount = CalculatePriceAfterDiscount(p.SellingPrice, p.Discount / 100),
                Profit = CalculateProfit(CalculatePriceAfterDiscount(p.SellingPrice, p.Discount / 100), p.ImportPrice),
                ProductId = productId
            }).ToList();

            return result;
        }

        public string  GetProductIdByProductItemId(string productItemId)
        {
            var productId = from p in db.Products
                            join pi in db.ProductItems on p.Id equals pi.ProductId
                            where pi.Id == productItemId
                            select p.Id;
            return productId.FirstOrDefault();
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

                                       StateId = p.StateId,
                                       // Convert the result to a list

                                   },

                                   Quantity = pi.Quantity,
                                   ImportPrice = pi.ImportPrice,
                                   SellingPrice = pi.SellingPrice,
                                   Discount = pi.Discount,

                               }).FirstOrDefault();

            return productItem;

        }
        public ProductItemModel GetProductDetails(string productId, string selectedRam, string selectedStorage)
        {

            {
                var productDetails = (from p in db.Products
                                      join pi in db.ProductItems on p.Id equals pi.ProductId
                                      join pc in db.ProductConfigurations on pi.Id equals pc.ProductItemId
                                      join vo in db.VariationOptions on pc.VariationOptionId equals vo.Id
                                      join va in db.Variations on vo.VariationId equals va.Id
                                      where p.Id == productId
                                      && db.ProductConfigurations.Any(pc2 =>
                                          pc2.ProductItemId == pi.Id &&
                                          vo.Value == selectedRam &&
                                          va.Name == "Ram")
                                      && db.ProductConfigurations.Any(pc3 =>
                                          pc3.ProductItemId == pi.Id &&
                                          vo.Value == selectedStorage &&
                                          va.Name == "Storage")
                                      select new ProductItemModel
                                      {
                                          Id = p.Id,
                                          ProductId = p.Id,
                                          Product = new ProductModel
                                          {
                                              Id = p.Id,
                                              Name = p.Name,
                                              Picture = p.Picture,
                                              Description = p.Description
                                          },
                                          SellingPrice = pi.SellingPrice
                                      }).FirstOrDefault();

                return productDetails;
            }
        }
        public List<Models.ProductModel> GetProductByBrand4(string brand, string excludeProductId)
        {
            // Check for null or empty brand and return an empty list if so
            if (string.IsNullOrWhiteSpace(brand))
            {
                return new List<Models.ProductModel>();
            }

            // Query to get products by brand, excluding the specified product (with string ID) and limiting the result to 4 items
            var query = (from p in db.Products
                         join c in db.Categories on p.CategoryId equals c.Id
                         join ps in db.ProductStates on p.StateId equals ps.Id
                         where c.Id.Contains(brand) && p.Id != excludeProductId // Exclude the input product by string ID
                         select new Models.ProductModel
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Description = p.Description,
                             Picture = p.Picture,
                             Quantity = (db.ProductItems
                                            .Where(pi => pi.ProductId == p.Id)
                                            .Sum(pi => (int?)pi.Quantity) ?? 0), // Handle NULL by converting to 0
                             CategoryName = c.Name,
                             ProductState = ps.Name
                         })
                         .Take(4); // Limit to 4 products

            var result = query.ToList(); // Execute the query

            // Fetch the minimum price for each product
            foreach (var item in result)
            {
                item.ProductItem = GetMinPrice(item.Id);
            }

            return result;
        }
        public string GetBrandId(string proId)
        {
            // Find the product by its ID
            var brandId = db.Products
                            .Where(p => p.Id == proId)
                            .Select(p => p.CategoryId) // Assuming CategoryId represents the Brand ID
                            .FirstOrDefault();

            // Return the brand ID (or null if not found)
            return brandId;
        }

        public void Disable(string productId)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                if (product.StateId != 2)
                {
                    product.StateId = 2;
                }
                else
                {
                    UpdateProductState(productId);
                }
                db.SaveChanges();
            }
        }

        public List<ProductLogModel> GetProductLog()
        {
            var result = db.ProductLogs
                .Select(p => new ProductLogModel
                {
                    Id = p.Id,
                    ProductItemId = p.QuantityLog != null ? p.QuantityLog.ProductItemId :
                                    p.PriceLog != null ? p.PriceLog.ProductItemId :
                                    p.DisocuntLog != null ? p.DisocuntLog.ProductItemId : null,
                    ActionType = p.QuantityLog != null ? "Quantity Change" :
                                 p.PriceLog != null ? "Price Change" :
                                 p.DisocuntLog != null ? "Discount Change" : "Unknown",
                    OldValue = p.QuantityLog != null ? p.QuantityLog.OldQuantity.ToString() :
                                p.PriceLog != null ? p.PriceLog.OldPrice.ToString() :
                                p.DisocuntLog != null ? p.DisocuntLog.OldDiscount.ToString() : null,
                    NewValue = p.QuantityLog != null ? p.QuantityLog.NewQuantity.ToString() :
                                p.PriceLog != null ? p.PriceLog.NewPrice.ToString() :
                                p.DisocuntLog != null ? p.DisocuntLog.NewDiscount.ToString() : null,
                    Date = p.QuantityLog != null ? p.QuantityLog.ChangeTimestamp :
                           p.PriceLog != null ? p.PriceLog.ChangeTimestamp :
                           p.DisocuntLog != null ? p.DisocuntLog.ChangeTimestamp : DateTime.MinValue
                }).OrderByDescending(p => p.Date)
                .ToList();

            return result;
        }
        

        public List<ProductLogModel> GetProductLog(string filter)
        {
            var logs = db.ProductLogs.AsQueryable();
            switch (filter)
            {
                case "1":
                    logs = logs.Where(p => p.QuantityLogId != null); // Quantity Change
                    break;
                case "2":
                    logs = logs.Where(p => p.PriceLogId != null); // Price Change
                    break;
                case "3":
                    logs = logs.Where(p => p.DisocuntLogId != null); // Discount Change
                    break;
                default:
                    // If "All" or no filter is selected, return all logs
                    break;
            }
            var result = logs.Select(p => new ProductLogModel
            {
                Id = p.Id,
                ProductItemId = p.QuantityLog != null ? p.QuantityLog.ProductItemId :
                                    p.PriceLog != null ? p.PriceLog.ProductItemId :
                                    p.DisocuntLog != null ? p.DisocuntLog.ProductItemId : null,
                ActionType = p.QuantityLog != null ? "Quantity Change" :
                                 p.PriceLog != null ? "Price Change" :
                                 p.DisocuntLog != null ? "Discount Change" : "Unknown",
                OldValue = p.QuantityLog != null ? p.QuantityLog.OldQuantity.ToString() :
                                p.PriceLog != null ? p.PriceLog.OldPrice.ToString() :
                                p.DisocuntLog != null ? p.DisocuntLog.OldDiscount.ToString() : null,
                NewValue = p.QuantityLog != null ? p.QuantityLog.NewQuantity.ToString() :
                                p.PriceLog != null ? p.PriceLog.NewPrice.ToString() :
                                p.DisocuntLog != null ? p.DisocuntLog.NewDiscount.ToString() : null,
                Date = p.QuantityLog != null ? p.QuantityLog.ChangeTimestamp :
                           p.PriceLog != null ? p.PriceLog.ChangeTimestamp :
                           p.DisocuntLog != null ? p.DisocuntLog.ChangeTimestamp : DateTime.MinValue
            }).OrderByDescending(p => p.Date)
                .ToList();

            return result;
        }

    }
}
