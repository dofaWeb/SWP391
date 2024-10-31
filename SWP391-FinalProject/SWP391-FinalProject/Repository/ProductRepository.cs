using K4os.Compression.LZ4.Streams.Adapters;
using Microsoft.CodeAnalysis;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class ProductRepository
    {
        private readonly DBContext db;

        public ProductRepository()
        {
            db = new DBContext();
        }

        public List<Models.ProductModel> GetProductsByKeyword(string keyword, string price, string category, string brand)
        {
            // Base SQL query with joins
            string query = @"SELECT p.Id, p.Name, p.Description, p.Picture, 
                            COALESCE(SUM(pi.Quantity), 0) AS Quantity,
                            c.Name AS CategoryName, c.Id AS CategoryId, ps.Name AS ProductState
                     FROM Product p
                     JOIN Category c ON p.category_id = c.Id
                     JOIN Product_State ps ON p.state_id = ps.Id
                     LEFT JOIN Product_Item pi ON pi.product_id = p.Id
                     WHERE 1 = 1
                     ";

            var parameters = new Dictionary<string, object>();

            // Keyword filtering
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += " AND (p.Name LIKE @keyword OR p.Name LIKE @keywordStart)";
                parameters.Add("@keyword", $"%{keyword}%");
                parameters.Add("@keywordStart", $"{keyword}%");
            }

            // Category filtering
            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category.Equals("Laptop", StringComparison.OrdinalIgnoreCase))
                {
                    query += " AND c.Id LIKE 'B0%'";
                }
                else if (category.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                {
                    query += " AND c.Id LIKE 'B1%'";
                }
            }

            // Brand filtering
            if (!string.IsNullOrWhiteSpace(brand))
            {
                query += " AND p.Name LIKE @brand";
                parameters.Add("@brand", $"%{brand}%");
            }

            query += " GROUP BY p.Id ORDER BY p.Name ASC"; // Group by product and order by name

            // Execute query and populate ProductModel list
            DataTable productTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            var result = new List<Models.ProductModel>();

            foreach (DataRow row in productTable.Rows)
            {
                var product = new Models.ProductModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    CategoryName = row["CategoryName"].ToString(),
                    CategoryId = row["CategoryId"].ToString(),
                    ProductState = row["ProductState"].ToString()
                };

                // Retrieve minimum price for each product
                string priceQuery = @"SELECT Id, selling_price, discount, 
	(selling_price - (selling_price * (discount / 100))) AS PriceAfterDiscount
                              FROM Product_Item 
                              WHERE product_id = @productId AND selling_price IS NOT NULL
                              ORDER BY selling_price ASC
                              LIMIT 1;";

                var priceParams = new Dictionary<string, object> { { "@productId", product.Id } };
                DataTable priceTable = DataAccess.DataAccess.ExecuteQuery(priceQuery, priceParams);

                if (priceTable.Rows.Count > 0)
                {
                    DataRow priceRow = priceTable.Rows[0];
                    product.ProductItem = new ProductItemModel
                    {
                        Id = priceRow["Id"].ToString(),
                        SellingPrice = Convert.ToDecimal(priceRow["selling_price"]),
                        Discount = Convert.ToDecimal(priceRow["Discount"]),
                        PriceAfterDiscount = Convert.ToDecimal(priceRow["PriceAfterDiscount"]),
                        Saving = Convert.ToDecimal(priceRow["selling_price"]) - Convert.ToDecimal(priceRow["PriceAfterDiscount"])
                    };
                }

                result.Add(product);
            }

            // Sort results based on price if specified
            if (!string.IsNullOrWhiteSpace(price))
            {
                if (price.Equals("Asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(p => p.ProductItem?.PriceAfterDiscount).ToList();
                }
                else if (price.Equals("Desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(p => p.ProductItem?.PriceAfterDiscount).ToList();
                }
            }

            return result;
        }


        public List<ProductModel> GetProductByBrand(string brand)
        {
            // Check for null or empty brand and return an empty list if so
            if (string.IsNullOrWhiteSpace(brand))
            {
                return new List<ProductModel>();
            }

            // Define the SQL query to get products for the specified brand
            string query = @"
        SELECT p.Id, 
       p.Name, 
       p.Description, 
       p.Picture, 
       COALESCE(SUM(pi.quantity), 0) AS TotalQuantity, 
       c.Name AS CategoryName, 
       ps.Name AS ProductState
FROM Product p
JOIN Category c ON p.category_id = c.Id
JOIN Product_State ps ON p.state_id = ps.Id
LEFT JOIN Product_Item pi  ON p.Id = pi.product_id
WHERE c.Name LIKE CONCAT('%', @brand, '%')
GROUP BY p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name;
    ";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@brand", brand }
    };

            // Execute the query and retrieve results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Convert the DataTable to a list of ProductModel objects
            var productList = new List<ProductModel>();
            foreach (DataRow row in resultTable.Rows)
            {
                var product = new ProductModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Quantity = Convert.ToInt32(row["TotalQuantity"]),
                    CategoryName = row["CategoryName"].ToString(),
                    ProductState = row["ProductState"].ToString(),
                    ProductItem = GetMinPrice(row["Id"].ToString()) // Get the minimum-priced item
                };

                productList.Add(product);
            }

            return productList;
        }


        public List<ProductModel> ProductsByCategory(string type)
        {
            // Check for null or empty type and return an empty list if so
            if (string.IsNullOrWhiteSpace(type))
            {
                return new List<ProductModel>();
            }

            // Determine the category keyword based on the type
            string keyword = type switch
            {
                "laptops" => "B0",
                "phones" => "B1",
                // Add more cases as needed
                _ => ""
            };

            // Return an empty list if keyword is still empty
            if (string.IsNullOrEmpty(keyword))
            {
                return new List<ProductModel>();
            }

            // Define the SQL query to get products by category
            string query = @"
        SELECT p.Id, 
               p.Name, 
               p.Description, 
               p.Picture, 
               COALESCE(SUM(pi.quantity), 0) AS TotalQuantity, 
               c.Name AS CategoryName, 
               ps.Name AS ProductState
        FROM Product p
        JOIN Category c ON p.category_id = c.Id
        JOIN Product_State ps ON p.state_id = ps.Id
        LEFT JOIN Product_Item pi ON p.Id = pi.product_id
        WHERE p.category_id LIKE CONCAT(@keyword, '%')
        GROUP BY p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name
    ";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@keyword", keyword }
    };

            // Execute the query and retrieve results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Convert the DataTable to a list of ProductModel objects
            var productList = new List<ProductModel>();
            foreach (DataRow row in resultTable.Rows)
            {
                var product = new ProductModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Quantity = Convert.ToInt32(row["TotalQuantity"]),
                    CategoryName = row["CategoryName"].ToString(),
                    ProductState = row["ProductState"].ToString(),
                    ProductItem = GetMinPrice(row["Id"].ToString()) // Get the minimum-priced item
                };

                productList.Add(product);
            }

            return productList;
        }


        public decimal? GetPrice(string ram, string storage, string productId)
        {
            // Define the SQL query
            string query = @"
                SELECT pi.selling_price, pi.Discount 
FROM Product p
JOIN Product_Item pi ON p.Id = pi.product_id
JOIN Product_Configuration pc ON pi.Id = pc.product_item_id
JOIN Variation_Option vo ON pc.variation_option_id = vo.Id
JOIN Variation va ON vo.variation_id = va.Id
WHERE vo.Value = @ram
  AND p.Id = @productId
  AND pi.Id IN (
      SELECT pi2.Id
      FROM Product p2
      JOIN Product_Item pi2 ON p2.Id = pi2.product_id
      JOIN Product_Configuration pc2 ON pi2.Id = pc2.product_item_id
      JOIN Variation_Option vo2 ON pc2.variation_option_id = vo2.Id
      JOIN Variation va2 ON vo2.variation_id = va2.Id
      WHERE vo2.Value = @storage AND p2.Id = @productId
  )
LIMIT 1";  // Limiting to 1 result for FirstOrDefault behavior

                    // Define parameters for the query
                    var parameters = new Dictionary<string, object>
            {
                { "@ram", ram },
                { "@storage", storage },
                { "@productId", productId }
            };

            // Execute the query
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if any results were returned
            if (result.Rows.Count > 0)
            {
                var row = result.Rows[0];
                decimal sellingPrice = row.Field<decimal>("selling_price");
                decimal discount = row.Field<decimal>("Discount");

                return ProductRepository.CalculatePriceAfterDiscount(sellingPrice, discount / 100);
            }

            return null; // No results found
        }

        public string GetProItemIdByVariation(string ram, string storage, string productId)
        {
            // Define the SQL query
            string query = @"
        SELECT pi.Id 
        FROM Product p
        JOIN Product_Item pi ON p.Id = pi.product_id
        JOIN Product_Configuration pc ON pi.Id = pc.product_item_id
        JOIN Variation_Option vo ON pc.variation_option_id = vo.Id
        JOIN Variation va ON vo.variation_id = va.Id
        WHERE vo.Value = @ram
          AND p.Id = @productId
          AND pi.Id IN (
              SELECT pi2.Id
              FROM Product p2
              JOIN Product_Item pi2 ON p2.Id = pi2.product_id
              JOIN Product_Configuration pc2 ON pi2.Id = pc2.product_item_id
              JOIN Variation_Option vo2 ON pc2.variation_option_id = vo2.Id
              JOIN Variation va2 ON vo2.variation_id = va2.Id
              WHERE vo2.Value = @storage AND p2.Id = @productId
          )
        LIMIT 1";  // Limiting to 1 result for FirstOrDefault behavior

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@ram", ram },
        { "@storage", storage },
        { "@productId", productId }
    };

            // Execute the query
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if any results were returned
            if (result.Rows.Count > 0)
            {
                return result.Rows[0]["Id"].ToString();  // Return the ProItemId as a string
            }

            return null; // No results found
        }


        public ProductItemModel GetMinPrice(string productId)
        {
            // Define the SQL query to get the product item with the minimum selling price
            string query = @"
        SELECT pi.Id,
       pi.Discount,
       pi.selling_price,
       (pi.selling_price * (1 - pi.Discount / 100)) AS PriceAfterDiscount,
       (pi.selling_price - (pi.selling_price * (1 - pi.Discount / 100))) AS Saving
FROM Product_Item pi
JOIN Product p ON p.Id = pi.product_id
WHERE pi.selling_price IS NOT NULL 
  AND p.Id = @productId
ORDER BY pi.selling_price ASC
LIMIT 1;
    ";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@productId", productId }
    };

            // Execute the query and retrieve the result as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if any result was returned
            if (resultTable.Rows.Count == 0)
            {
                return null; // No product item found
            }

            // Map the first row of the result to ProductItemModel
            var row = resultTable.Rows[0];
            ProductItemModel minPriceProductItem = new ProductItemModel
            {
                Id = row["Id"].ToString(),
                Discount = Convert.ToDecimal(row["Discount"]),
                SellingPrice = Convert.ToDecimal(row["selling_price"]),
                PriceAfterDiscount = Convert.ToDecimal(row["PriceAfterDiscount"]),
                Saving = Convert.ToDecimal(row["Saving"])
            };

            return minPriceProductItem;
        }


        public List<Models.ProductModel> GetTopSellingProduct()
        {
            // Step 1: Get top 4 best-selling products where the order state is approved (state_id = 2)
            string topProductsQuery = @"
            SELECT p.Id AS ProductId, p.Name AS ProductName, COUNT(oi.order_id) AS TotalPurchases
            FROM Product p
            JOIN Product_Item pi ON p.Id = pi.product_id
            JOIN Order_Item oi ON pi.Id = oi.product_item_id
            JOIN SWP391.Order o ON oi.order_id = o.Id  -- Escape 'Order' with backticks
            WHERE o.state_id = 2 AND p.state_id = 1
            GROUP BY p.Id, p.Name
            ORDER BY TotalPurchases DESC
            LIMIT 4";


            // Execute the query to get top products
            DataTable topProductsTable = DataAccess.DataAccess.ExecuteQuery(topProductsQuery);
            var topProducts = new List<dynamic>();

            // Convert DataTable to a list of dynamic objects for easier access
            foreach (DataRow row in topProductsTable.Rows)
            {
                topProducts.Add(new
                {
                    ProductId = row["ProductId"],
                    ProductName = row["ProductName"],
                    TotalPurchases = row["TotalPurchases"]
                });
            }

            // Step 2: Get all products and related data (category, state, quantity)
            var productModels = new List<Models.ProductModel>();

            // Prepare to collect product IDs for the next query
            var productIds = topProducts.Select(tp => tp.ProductId).ToList();

            if (productIds.Count > 0)
            {
                // If productIds are strings, ensure they are correctly quoted for the SQL query
                string productQuery = @"
    SELECT p.Id, p.Name, p.Description, p.Picture, c.Name AS CategoryName, ps.Name AS ProductState
    FROM Product p
    JOIN Category c ON p.category_id = c.Id
    JOIN Product_State ps ON p.state_id = ps.Id
    WHERE p.Id IN ('" + string.Join("', '", productIds) + "')"; // Correctly format the IN clause

                // Execute the query to get product details
                DataTable productTable = DataAccess.DataAccess.ExecuteQuery(productQuery);

                foreach (DataRow row in productTable.Rows)
                {
                    var productModel = new Models.ProductModel
                    {
                        Id = (string)row["Id"],
                        Name = (string)row["Name"],
                        Description = (string)row["Description"],
                        Picture = (string)row["Picture"],
                        Quantity = (db.ProductItems
                                    .Where(pi => pi.ProductId == (string)row["Id"])
                                    .Sum(pi => (int?)pi.Quantity) ?? 0), // Handling NULL by converting to 0
                        CategoryName = (string)row["CategoryName"],
                        ProductState = (string)row["ProductState"]
                    };

                    // Step 3: Get the minimum price for each product
                    productModel.ProductItem = GetMinPrice(productModel.Id); // Assuming this method gets the minimum price

                    // Add the product model to the list
                    productModels.Add(productModel);
                }
            }

            return productModels;
        }


        public List<Models.ProductModel> GetAllProduct()
        {
            // SQL query to retrieve all products with their categories and states
            string productQuery = @"
    SELECT p.Id, p.Name, p.Description, p.Picture, c.Name AS CategoryName, ps.Name AS ProductState
    FROM Product p
    JOIN Category c ON p.category_id = c.Id
    JOIN Product_State ps ON p.state_id = ps.Id";

            // Execute the query to get product details
            DataTable productTable = DataAccess.DataAccess.ExecuteQuery(productQuery);
            var productModels = new List<Models.ProductModel>();

            foreach (DataRow row in productTable.Rows)
            {
                var productModel = new Models.ProductModel
                {
                    Id = (string)row["Id"],
                    Name = (string)row["Name"],
                    Description = (string)row["Description"],
                    Picture = (string)row["Picture"],
                    Quantity = (db.ProductItems
                                .Where(pi => pi.ProductId == (string)row["Id"])
                                .Sum(pi => (int?)pi.Quantity) ?? 0), // Handling NULL by converting to 0
                    CategoryName = (string)row["CategoryName"],
                    ProductState = (string)row["ProductState"]
                };

                // Get the minimum price for each product
                productModel.ProductItem = GetMinPrice(productModel.Id); // Assuming this method gets the minimum price

                // Add the product model to the list
                productModels.Add(productModel);
            }

            return productModels;
        }

        public void UpdateProductState(string productId)
        {
            // Check the total quantity and the current state in one query
            var query = @"
        SELECT SUM(pi.Quantity) AS TotalQuantity, p.state_id
        FROM Product p
        LEFT JOIN Product_Item pi ON p.Id = pi.product_id
        WHERE p.Id = 'P0000003'
        GROUP BY p.Id";

            var parameters = new Dictionary<string, object>
    {
        { "@productId", productId }
    };

            var resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count > 0)
            {
                var totalQuantity = resultTable.Rows[0]["TotalQuantity"] as int? ?? 0; // Handle NULLs
                var productToUpdate = resultTable.Rows[0]["state_id"] as int?; // Get the current state

                // Update state based on quantity
                int newStateId = totalQuantity == 0 ? 3 : 1;

                if (productToUpdate != null && (int)productToUpdate != newStateId)
                {
                    // Perform update only if the state needs to change
                    string updateQuery = "UPDATE Product SET state_id = @newStateId WHERE Id = @productId";
                    var updateParameters = new Dictionary<string, object>
            {
                { "@newStateId", newStateId },
                { "@productId", productId }
            };

                    DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
                }
            }
        }


        public string getNewProductID()
        {
            var query = "SELECT ID FROM Product Order By Id DESC LIMIT 1;";
            DataTable dataTable = DataAccess.DataAccess.ExecuteQuery(query);



            string lastId = dataTable.Rows[0]["Id"].ToString();
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

        public List<Models.ProductStateModel> GetAllProductState()
        {
            
            List<Models.ProductStateModel> productStates = new List<Models.ProductStateModel>();

            // MySQL query to retrieve all product states
            string query = "Select * from Product_State";
            DataTable categoryTable = DataAccess.DataAccess.ExecuteQuery(query);
            foreach (DataRow row in categoryTable.Rows)
            {
                productStates.Add(new Models.ProductStateModel
                {
                    Id = int.Parse(row["Id"].ToString()),
                    Name = row["Name"].ToString()
                });
            }
            return productStates;

        }

        public void AddProduct(Models.ProductModel model, IFormFile pictureUpload)
        {
            // Construct the INSERT query
            string insertQuery = @"
        INSERT INTO Product (Id, Name, Category_Id, State_Id, Description, Picture)
        VALUES (@Id, @Name, @CategoryId, @StateId, @Description, @Picture)";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@Name", model.Name },
        { "@CategoryId", model.CategoryId },
        { "@StateId", 3 }, // Assuming state 3 is for a new product
        { "@Description", model.Description },
        { "@Picture", pictureUpload != null ? MyUtil.UpLoadPicture(pictureUpload) : null }
    };

            // Execute the INSERT query
            DataAccess.DataAccess.ExecuteNonQuery(insertQuery, parameters);
        }


        public int? GetProductQuantityById(string id)
        {
            // Construct the SELECT query
            string query = @"
        SELECT SUM(pi.Quantity) AS TotalQuantity
        FROM Product p
        JOIN Product_Item pi ON p.Id = pi.product_id
        WHERE p.Id = @ProductId";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
    {
        { "@ProductId", id }
    };

            // Execute the query
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Extract the quantity from the result
            if (resultTable.Rows.Count > 0)
            {
                return resultTable.Rows[0].Field<int?>("TotalQuantity");
            }

            return null; // Return null if no rows are found
        }


        public void UpdateProduct(ProductModel model, IFormFile pictureUpload)
        {
            // Check if the product exists in the database
            string checkQuery = "SELECT COUNT(1) FROM SWP391.Product WHERE Id = @Id";
            var checkParameters = new Dictionary<string, object> { { "@Id", model.Id } };
            DataTable checkResult = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (checkResult.Rows[0][0].ToString() == "0")
            {
                throw new Exception("Product not found!");
            }

            // Determine the StateId based on product quantity
            int stateId = GetProductQuantityById(model.Id) > 0 ? model.StateId : 3;

            // Handle picture upload, if provided
            string picturePath = null;
            if (pictureUpload != null)
            {
                // Upload the new picture
                picturePath = MyUtil.UpLoadPicture(pictureUpload);

                // Delete old picture
                string deleteOldPicturePathQuery = "SELECT Picture FROM Product WHERE Id = @Id";
                DataTable oldPictureResult = DataAccess.DataAccess.ExecuteQuery(deleteOldPicturePathQuery, checkParameters);
                string oldPicturePath = oldPictureResult.Rows[0]["Picture"].ToString();

                MyUtil.DeletePicture(oldPicturePath);
            }

            // Prepare SQL update query
            string updateQuery = @"
    UPDATE Product
    SET Name = @Name,
        Category_Id = @CategoryId,
        Description = @Description,
        State_Id = @StateId,
        Picture = IF(@Picture IS NOT NULL, @Picture, Picture) -- Only update picture if new picture provided
    WHERE Id = @Id;";

            // Define parameters for update query
            var updateParameters = new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@Name", model.Name },
        { "@CategoryId", model.CategoryId },
        { "@Description", model.Description },
        { "@StateId", stateId }
    };

            // Include the picture only if it exists
            if (picturePath != null)
            {
                updateParameters.Add("@Picture", picturePath);
            }
            else
            {
                // If no new picture, set a default to avoid null parameter
                updateParameters.Add("@Picture", DBNull.Value);
            }

            // Execute the update query within a transaction
            using var transaction = new System.Transactions.TransactionScope();
            try
            {
                DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
                transaction.Complete();
            }
            catch (Exception ex)
            {
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
            // SQL Query
            string query = @"
        SELECT TOP 1
            p.Id AS Id,
            p.Id AS ProductId,
            p.Name AS ProductName,
            p.Picture AS ProductPicture,
            p.Description AS ProductDescription,
            pi.selling_price AS SellingPrice
        FROM 
            SWP391.Product p
        JOIN 
            SWP391.Product_Item pi ON p.Id = pi.product_id
        JOIN 
            SWP391.Product_Configuration pc ON pi.Id = pc.product_item_id
        JOIN 
            SWP391.Variation_Option vo ON pc.variation_option_id = vo.Id
        JOIN 
            SWP391.Variation va ON vo.variation_id = va.Id
        WHERE 
            p.Id = @productId
            AND EXISTS (
                SELECT 1
                FROM SWP391.Product_Configuration pc2
                JOIN SWP391.Variation_Option vo2 ON pc2.variation_option_id = vo2.Id
                JOIN SWP391.Variation va2 ON vo2.variation_id = va2.Id
                WHERE pc2.product_item_id = pi.Id
                  AND vo2.Value = @selectedRam
                  AND va2.Name = 'Ram'
            )
            AND EXISTS (
                SELECT 1
                FROM SWP391.Product_Configuration pc3
                JOIN SWP391.Variation_Option vo3 ON pc3.variation_option_id = vo3.Id
                JOIN SWP391.Variation va3 ON vo3.variation_id = va3.Id
                WHERE pc3.product_item_id = pi.Id
                  AND vo3.Value = @selectedStorage
                  AND va3.Name = 'Storage'
            );";

            // Define parameters
            var parameters = new Dictionary<string, object>
    {
        { "@productId", productId },
        { "@selectedRam", selectedRam },
        { "@selectedStorage", selectedStorage }
    };

            // Execute query and get result
            DataTable productTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (productTable.Rows.Count == 0)
            {
                return null; // No product found
            }

            DataRow row = productTable.Rows[0];

            // Map result to ProductItemModel
            var productDetails = new ProductItemModel
            {
                Id = row["Id"].ToString(),
                ProductId = row["ProductId"].ToString(),
                Product = new ProductModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["ProductName"].ToString(),
                    Picture = row["ProductPicture"].ToString(),
                    Description = row["ProductDescription"].ToString()
                },
                SellingPrice = Convert.ToDecimal(row["SellingPrice"])
            };

            return productDetails;
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
