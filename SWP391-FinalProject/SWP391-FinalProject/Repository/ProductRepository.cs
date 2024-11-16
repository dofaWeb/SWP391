using K4os.Compression.LZ4.Streams.Adapters;
using Microsoft.CodeAnalysis;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Xml.Linq;

namespace SWP391_FinalProject.Repository
{
    public class ProductRepository
    {


        public ProductRepository()
        {

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
            // Step 1: Get top 4 best-selling products with their details
            string topProductsQuery = @"
        SELECT p.Id AS ProductId, 
               p.Name AS ProductName, 
               p.Description, 
               p.Picture, 
               c.Name AS CategoryName, 
               ps.Name AS ProductState, 
               COUNT(oi.order_id) AS TotalPurchases,
               SUM(pi.Quantity) AS TotalQuantity
        FROM Product p
        JOIN Product_Item pi ON p.Id = pi.product_id
        JOIN Order_Item oi ON pi.Id = oi.product_item_id
        JOIN `Order` o ON oi.order_id = o.Id  -- Escape 'Order' with backticks
        JOIN Category c ON p.category_id = c.Id
        JOIN Product_State ps ON p.state_id = ps.Id
        WHERE o.state_id = 2 AND p.state_id = 1
        GROUP BY p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name
        ORDER BY TotalPurchases DESC
        LIMIT 4";

            // Execute the query to get top products
            DataTable topProductsTable = DataAccess.DataAccess.ExecuteQuery(topProductsQuery);
            var productModels = new List<Models.ProductModel>();

            // Convert DataTable to a list of ProductModel
            foreach (DataRow row in topProductsTable.Rows)
            {
                var productModel = new Models.ProductModel
                {
                    Id = (string)row["ProductId"],
                    Name = (string)row["ProductName"],
                    Description = (string)row["Description"],
                    Picture = (string)row["Picture"],
                    Quantity = row["TotalQuantity"] != DBNull.Value ? Convert.ToInt32(row["TotalQuantity"]) : 0, // Handling NULLs
                    CategoryName = (string)row["CategoryName"],
                    ProductState = (string)row["ProductState"]
                };

                // Step 2: Get the minimum price for each product
                productModel.ProductItem = GetMinPrice(productModel.Id); // Assuming this method gets the minimum price

                // Add the product model to the list
                productModels.Add(productModel);
            }

            return productModels;
        }



        public List<Models.ProductModel> GetAllProduct()
        {
            // SQL query to retrieve all products with their categories, states, total quantity, and minimum price
            string productQuery = @"
        SELECT p.Id AS ProductId, 
               p.Name AS ProductName, 
               p.Description, 
               p.Picture, 
               c.Name AS CategoryName, 
               ps.Name AS ProductState,
               COALESCE(SUM(pi.Quantity), 0) AS TotalQuantity,
               MIN(pi.selling_price) AS MinPrice -- Assuming you have a Price column in Product_Item
        FROM Product p
        JOIN Category c ON p.category_id = c.Id
        JOIN Product_State ps ON p.state_id = ps.Id
        LEFT JOIN Product_Item pi ON p.Id = pi.product_id -- Use LEFT JOIN to include products with no items
        
        GROUP BY p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name";

            // Execute the query to get product details
            DataTable productTable = DataAccess.DataAccess.ExecuteQuery(productQuery);
            var productModels = new List<Models.ProductModel>();

            foreach (DataRow row in productTable.Rows)
            {
                var productModel = new Models.ProductModel
                {
                    Id = (string)row["ProductId"],
                    Name = (string)row["ProductName"],
                    Description = (string)row["Description"],
                    Picture = (string)row["Picture"],
                    Quantity = row["TotalQuantity"] != DBNull.Value ? Convert.ToInt32(row["TotalQuantity"]) : 0, // Convert to int
                    CategoryName = (string)row["CategoryName"],
                    ProductState = (string)row["ProductState"],
                };


                productModel.ProductItem = GetMinPrice(productModel.Id);

                // Add the product model to the list
                productModels.Add(productModel);
            }

            return productModels;
        }


        public void UpdateProductState(string productId)
        {
            // Check the total quantity and the current state in one query
            var query = @"
        SELECT IFNULL(SUM(pi.Quantity), 0) AS TotalQuantity, p.state_id
        FROM Product p
        LEFT JOIN Product_Item pi ON p.Id = pi.product_id
        WHERE p.Id = @productId
        GROUP BY p.Id";

            var parameters = new Dictionary<string, object>
    {
        { "@productId", productId }
    };

            var resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count > 0)
            {
                var totalQuantity = (decimal)resultTable.Rows[0]["TotalQuantity"];
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

        public bool AddProduct(Models.ProductModel model, IFormFile pictureUpload)
        {
            string check = "SELECT * FROM Product Where Name = @Name And category_Id = @Category";
            var parameter = new Dictionary<string, object> {
                {"@Name", model.Name },
                { "@Category", model.CategoryId }
            };
            var result = DataAccess.DataAccess.ExecuteQuery(check, parameter);
            if (result.Rows.Count == 0)
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
                return true;
            }
            return false;
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
                return int.Parse(resultTable.Rows[0]["TotalQuantity"].ToString());
            }

            return null; // Return null if no rows are found
        }


        public void UpdateProduct(ProductModel model, IFormFile pictureUpload)
        {
            // Check if the product exists in the database
            string checkQuery = "SELECT COUNT(1) FROM `Product` WHERE Id = @Id";
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
            // Define the SQL query to select ProductItem IDs where ProductId matches
            string query = "SELECT Id FROM Product_Item WHERE Product_Id = @ProductId";

            // Create a dictionary to hold parameters
            var parameters = new Dictionary<string, object>
    {
        { "@ProductId", productId }
    };

            // Execute the query and retrieve the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Convert the DataTable results to a List<string>
            var result = new List<string>();
            foreach (DataRow row in resultTable.Rows)
            {
                result.Add(row["Id"].ToString());
            }

            return result;
        }



        public Models.ProductModel GetProductById(string id)
        {
            // Define the SQL query to select the product details, including category and product state
            string query = @"
        SELECT 
            p.Id AS ProductId, 
            p.Name AS ProductName, 
            p.Description, 
            p.Picture, 
            c.Name AS CategoryName, 
            ps.Name AS ProductState,
            c.Id AS CategoryId,
            COALESCE(SUM(pi.Quantity), 0) AS Quantity -- Calculate total quantity or 0 if NULL
        FROM 
            Product p
        JOIN 
            Category c ON p.Category_Id = c.Id
        JOIN 
            Product_State ps ON p.State_Id = ps.Id
        LEFT JOIN 
            Product_Item pi ON pi.Product_Id = p.Id
        WHERE 
            p.Id = @Id
        GROUP BY 
            p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name, c.Id";

            // Create a dictionary to hold the parameter
            var parameters = new Dictionary<string, object>
    {
        { "@Id", id }
    };

            // Execute the query and retrieve the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Convert the result to a ProductModel instance
            if (resultTable.Rows.Count == 0)
            {
                return null; // Return null if no product was found
            }

            DataRow row = resultTable.Rows[0];
            var product = new Models.ProductModel
            {
                Id = row["ProductId"].ToString(),
                Name = row["ProductName"].ToString(),
                Description = row["Description"].ToString(),
                Picture = row["Picture"].ToString(),
                Quantity = Convert.ToInt32(row["Quantity"]),
                CategoryName = row["CategoryName"].ToString(),
                ProductState = row["ProductState"].ToString(),
                CategoryId = row["CategoryId"].ToString()
            };

            return product;
        }


        public string GetProductVariationOption(string productItemId, string option)
        {
            // Define the SQL query to get the variation option value
            string query = @"
               SELECT vo.Value
                FROM Product_Configuration pc
                JOIN Variation_Option vo ON pc.variation_option_id = vo.Id
                JOIN Variation va ON vo.variation_id = va.Id
                WHERE pc.product_item_id = @ProductItemId
                AND va.Name = @Option
                LIMIT 1"; // Using LIMIT 1 to ensure a single result

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@ProductItemId", productItemId },
        { "@Option", option }
    };

            // Execute the query and retrieve the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Retrieve the value or return an empty string if no result is found
            if (resultTable.Rows.Count == 0)
            {
                return string.Empty;
            }

            return resultTable.Rows[0]["Value"].ToString();
        }


        public static decimal CalculatePriceAfterDiscount(decimal? SellingPrice, decimal? discount)
        {
            try
            {
                if (SellingPrice < 0)
                    throw new ArgumentException("Selling price cannot be negative.");
                if (discount < 0 || discount > 1)
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
            // Query to get product items for a given ProductId
            string productItemsQuery = @"
         SELECT Id, Quantity, import_price, selling_price, Discount
         FROM Product_Item
         WHERE product_id = @ProductId";

            var parameters = new Dictionary<string, object> { { "@ProductId", productId } };
            DataTable productItemsTable = DataAccess.DataAccess.ExecuteQuery(productItemsQuery, parameters);

            // Prepare the result list
            var result = new List<ProductItemModel>();

            foreach (DataRow row in productItemsTable.Rows)
            {
                // Get basic fields from ProductItems
                var productItemId = row["Id"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);
                decimal importPrice = Convert.ToDecimal(row["import_price"]);
                decimal sellingPrice = Convert.ToDecimal(row["selling_price"]);
                decimal discount = Convert.ToDecimal(row["Discount"]);

                // Retrieve the variation options for Ram and Storage
                string ram = GetProductVariationOption(productItemId, "Ram");
                string storage = GetProductVariationOption(productItemId, "Storage");

                // Calculate PriceAfterDiscount and Profit
                decimal priceAfterDiscount = CalculatePriceAfterDiscount(sellingPrice, discount / 100);
                decimal? profit = CalculateProfit(priceAfterDiscount, importPrice);

                // Create the ProductItemModel and add it to the result list
                var productItem = new ProductItemModel
                {
                    Id = productItemId,
                    Quantity = quantity,
                    ImportPrice = importPrice,
                    SellingPrice = sellingPrice,
                    Ram = ram,
                    Storage = storage,
                    Discount = discount,
                    PriceAfterDiscount = priceAfterDiscount,
                    Profit = profit,
                    ProductId = productId
                };

                result.Add(productItem);
            }

            return result;
        }


        public string GetProductIdByProductItemId(string productItemId)
        {
            // SQL query to retrieve ProductId based on ProductItemId
            string query = @"
         SELECT p.Id
        FROM Product p
        JOIN Product_Item pi ON p.Id = pi.product_id
        WHERE pi.Id =  @ProductItemId";

            // Define parameter for ProductItemId
            var parameters = new Dictionary<string, object>
    {
        { "@ProductItemId", productItemId }
    };

            // Execute the query and get the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Return the ProductId if found; otherwise, return null
            return resultTable.Rows.Count > 0 ? resultTable.Rows[0]["Id"].ToString() : null;
        }



        public ProductItemModel GetProductItemById(string productId)
        {
            // Define SQL query to retrieve the product item by productId
            string query = @"
        SELECT 
    pi.Id AS ProductItemId,
    pi.product_id,
    pi.Quantity,
    pi.import_price,
    pi.selling_price,
    pi.Discount,
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.Picture,
    p.Description,
    p.category_id,
    c.Name AS CategoryName,
    p.state_id
FROM 
    Product_Item pi
JOIN 
    Product p ON pi.product_id = p.Id
LEFT JOIN 
    Category c ON p.category_id = c.Id
WHERE 
    p.Id = @ProductId
LIMIT 1;"; // Only get the first matching result

            // Define the parameter for productId
            var parameters = new Dictionary<string, object>
    {
        { "@ProductId", productId }
    };

            // Execute the query
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if a result was returned
            if (resultTable.Rows.Count == 0)
            {
                return null; // No matching product item found
            }

            // Map the result to ProductItemModel
            DataRow row = resultTable.Rows[0];
            var productItem = new ProductItemModel
            {
                Id = row["ProductItemId"].ToString(),
                ProductId = row["product_id"].ToString(),
                Quantity = Convert.ToInt32(row["Quantity"]),
                ImportPrice = Convert.ToDecimal(row["import_price"]),
                SellingPrice = Convert.ToDecimal(row["selling_price"]),
                Discount = Convert.ToDecimal(row["Discount"]),
                Product = new ProductModel
                {
                    Id = row["product_id"].ToString(),
                    Name = row["ProductName"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Description = row["Description"].ToString(),
                    CategoryId = row["category_id"].ToString(),
                    CategoryName = row["CategoryName"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"])
                }
            };

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
            Product p
        JOIN 
            Product_Item pi ON p.Id = pi.product_id
        JOIN 
            Product_Configuration pc ON pi.Id = pc.product_item_id
        JOIN 
            Variation_Option vo ON pc.variation_option_id = vo.Id
        JOIN 
            Variation va ON vo.variation_id = va.Id
        WHERE 
            p.Id = @productId
            AND EXISTS (
                SELECT 1
                FROM Product_Configuration pc2
                JOIN Variation_Option vo2 ON pc2.variation_option_id = vo2.Id
                JOIN Variation va2 ON vo2.variation_id = va2.Id
                WHERE pc2.product_item_id = pi.Id
                  AND vo2.Value = @selectedRam
                  AND va2.Name = 'Ram'
            )
            AND EXISTS (
                SELECT 1
                FROM Product_Configuration pc3
                JOIN Variation_Option vo3 ON pc3.variation_option_id = vo3.Id
                JOIN Variation va3 ON vo3.variation_id = va3.Id
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

            // Define SQL query to get products by brand, excluding the specified product, limited to 4 items
            string query = @"
                SELECT 
            p.Id,
            p.Name,
            p.Description,
            p.Picture,
            c.Name AS CategoryName,
            ps.Name AS ProductState,
            COALESCE(SUM(pi.Quantity), 0) AS Quantity
        FROM 
            Product p
        JOIN 
            Category c ON p.category_id = c.Id
        JOIN 
            Product_State ps ON p.state_id = ps.Id
        LEFT JOIN 
            Product_Item pi ON pi.product_id = p.Id
        WHERE 
            c.Id LIKE CONCAT('%', @Brand, '%') 
            AND p.Id != @ExcludeProductId
        GROUP BY 
            p.Id
        LIMIT 4";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Brand", brand },
        { "@ExcludeProductId", excludeProductId }
    };

            // Execute the query
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Map the result to List<ProductModel>
            var result = new List<Models.ProductModel>();
            foreach (DataRow row in resultTable.Rows)
            {
                var product = new Models.ProductModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    CategoryName = row["CategoryName"].ToString(),
                    ProductState = row["ProductState"].ToString()
                };

                // Retrieve minimum price for the product and assign to ProductItem
                product.ProductItem = GetMinPrice(product.Id);

                result.Add(product);
            }

            return result;
        }

        public string GetBrandId(string proId)
        {
            // Define SQL query to find the CategoryId (assumed to be BrandId) by ProductId
            string query = @"
        SELECT Category_Id 
        FROM Product 
        WHERE Id = @ProductId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@ProductId", proId }
    };

            // Execute the query and get the result as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if there is a result and retrieve the CategoryId
            if (resultTable.Rows.Count > 0)
            {
                return resultTable.Rows[0]["Category_Id"].ToString();
            }

            // Return null if no matching product is found
            return null;
        }


        public void Disable(string productId)
        {
            // SQL query to get the current StateId of the specified product
            string getStateIdQuery = "SELECT State_Id FROM Product WHERE Id = @ProductId";
            var parameters = new Dictionary<string, object> { { "@ProductId", productId } };
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(getStateIdQuery, parameters);

            // Check if the product exists
            if (resultTable.Rows.Count > 0)
            {
                int currentStateId = Convert.ToInt32(resultTable.Rows[0]["State_Id"]);

                if (currentStateId != 2)
                {
                    // Update StateId to 2 if the current state is not 2
                    string updateStateIdQuery = "UPDATE Product SET State_Id = 2 WHERE Id = @ProductId";
                    DataAccess.DataAccess.ExecuteNonQuery(updateStateIdQuery, parameters);
                }
                else
                {
                    // If the StateId is already 2, call the UpdateProductState method
                    UpdateProductState(productId);
                }
            }
        }


        public List<ProductLogModel> GetProductLog()
        {
            // Define the SQL query to fetch product logs with joins for related logs
            var query = @"
           SELECT pl.Id AS LogId,
               COALESCE(ql.Product_Item_Id, prl.Product_Item_Id, dl.Product_Item_Id) AS ProductItemId,
               CASE 
                   WHEN ql.product_item_id IS NOT NULL THEN 'Quantity Change'
                   WHEN prl.product_item_id IS NOT NULL THEN 'Price Change'
                   WHEN dl.product_item_id IS NOT NULL THEN 'Discount Change'
                   ELSE 'Unknown'
               END AS ActionType,
               COALESCE(ql.old_quantity, prl.old_price, dl.old_Discount) AS OldValue,
               COALESCE(ql.new_quantity, prl.new_price, dl.new_discount) AS NewValue,
               COALESCE(ql.change_timestamp, prl.change_timestamp, dl.change_timestamp) AS ChangeDate
        FROM Product_Log pl
        LEFT JOIN Quantity_Log ql ON pl.quantity_log_id = ql.id
        LEFT JOIN Price_Log prl ON prl.id = pl.price_log_id
        LEFT JOIN Discount_Log dl ON pl.disocunt_log_id = dl.Id
        ORDER BY ChangeDate DESC;"; // Order by date

            // Execute the query and map results to ProductLogModel
            var result = new List<ProductLogModel>();
            var dataTable = DataAccess.DataAccess.ExecuteQuery(query);

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(new ProductLogModel
                {
                    Id = row["LogId"].ToString(),
                    ProductItemId = row["ProductItemId"]?.ToString(),
                    ActionType = row["ActionType"]?.ToString(),
                    OldValue = row["OldValue"]?.ToString(),
                    NewValue = row["NewValue"]?.ToString(),
                    Date = row["ChangeDate"] != DBNull.Value ? Convert.ToDateTime(row["ChangeDate"]) : DateTime.MinValue
                });
            }

            return result;
        }

        public List<ProductLogModel> GetProductLog(string filter)
        {
            // Base SQL query for fetching product logs with necessary joins
            var query = @"
        SELECT pl.Id AS LogId,
               COALESCE(ql.Product_Item_Id, prl.Product_Item_Id, dl.Product_Item_Id) AS ProductItemId,
               CASE 
                   WHEN ql.product_item_id IS NOT NULL THEN 'Quantity Change'
                   WHEN prl.product_item_id IS NOT NULL THEN 'Price Change'
                   WHEN dl.product_item_id IS NOT NULL THEN 'Discount Change'
                   ELSE 'Unknown'
               END AS ActionType,
               COALESCE(ql.old_quantity, prl.old_price, dl.old_discount) AS OldValue,
               COALESCE(ql.new_quantity, prl.new_price, dl.new_discount) AS NewValue,
               COALESCE(ql.change_timestamp, prl.change_timestamp, dl.change_timestamp) AS ChangeDate
        FROM Product_Log pl
        LEFT JOIN Quantity_Log ql ON pl.quantity_log_id = ql.id
        LEFT JOIN Price_Log prl ON prl.id = pl.price_log_id
        LEFT JOIN Discount_Log dl ON pl.disocunt_log_id = dl.id";

            // Modify the query based on the filter
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "1": // Quantity Change
                        query += " WHERE ql.product_item_id IS NOT NULL";
                        break;
                    case "2": // Price Change
                        query += " WHERE prl.product_item_id IS NOT NULL";
                        break;
                    case "3": // Discount Change
                        query += " WHERE dl.product_item_id IS NOT NULL";
                        break;
                    default:
                        // No additional filtering
                        break;
                }
            }

            query += " ORDER BY ChangeDate DESC;"; // Order by date

            // Execute the query and map results to ProductLogModel
            var result = new List<ProductLogModel>();
            var dataTable = DataAccess.DataAccess.ExecuteQuery(query);

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(new ProductLogModel
                {
                    Id = row["LogId"].ToString(),
                    ProductItemId = row["ProductItemId"]?.ToString(),
                    ActionType = row["ActionType"]?.ToString(),
                    OldValue = row["OldValue"]?.ToString(),
                    NewValue = row["NewValue"]?.ToString(),
                    Date = row["ChangeDate"] != DBNull.Value ? Convert.ToDateTime(row["ChangeDate"]) : DateTime.MinValue
                });
            }

            return result;
        }

        public List<Models.ProductModel> GetAllProductByKeyword(string keyword)
        {
            // SQL query to retrieve all products with their categories, states, total quantity, and minimum price
            string productQuery = @"
        SELECT p.Id AS ProductId, 
               p.Name AS ProductName, 
               p.Description, 
               p.Picture, 
               c.Name AS CategoryName, 
               ps.Name AS ProductState,
               COALESCE(SUM(pi.Quantity), 0) AS TotalQuantity,
               MIN(pi.selling_price) AS MinPrice -- Assuming you have a Price column in Product_Item
        FROM Product p
        JOIN Category c ON p.category_id = c.Id
        JOIN Product_State ps ON p.state_id = ps.Id
        LEFT JOIN Product_Item pi ON p.Id = pi.product_id -- Use LEFT JOIN to include products with no items
        WHERE (p.Name LIKE @keyword OR p.Name LIKE @keywordStart OR p.Id LIKE @keyword OR p.Id LIKE @keywordStart)
        GROUP BY p.Id, p.Name, p.Description, p.Picture, c.Name, ps.Name";



            var parameters = new Dictionary<string, object>();
            parameters.Add("@keyword", $"%{keyword}%");
            parameters.Add("@keywordStart", $"{keyword}%");


            // Execute the query to get product details
            DataTable productTable = DataAccess.DataAccess.ExecuteQuery(productQuery, parameters);
            var productModels = new List<Models.ProductModel>();

            foreach (DataRow row in productTable.Rows)
            {
                var productModel = new Models.ProductModel
                {
                    Id = (string)row["ProductId"],
                    Name = (string)row["ProductName"],
                    Description = (string)row["Description"],
                    Picture = (string)row["Picture"],
                    Quantity = row["TotalQuantity"] != DBNull.Value ? Convert.ToInt32(row["TotalQuantity"]) : 0, // Convert to int
                    CategoryName = (string)row["CategoryName"],
                    ProductState = (string)row["ProductState"],
                };


                productModel.ProductItem = GetMinPrice(productModel.Id);

                // Add the product model to the list
                productModels.Add(productModel);
            }

            return productModels;
        }

        public bool DeleteProduct(string id)
        {
            var proItemList = GetProductItemIdByProductId(id);
            ProductItemRepository proItemRepo = new ProductItemRepository();
            var check = false;
            foreach (var item in proItemList)
            {
                //proItemRepo.CheckExistVariation
                string checkOrder = "SELECT * FROM product_item pi JOIN order_item oi ON pi.id = oi.product_item_id where pi.id = @proItemId";
                Dictionary<string, object> parameter = new Dictionary<string, object>
                {
                    { "@proItemId", item }
                };
                check = DataAccess.DataAccess.ExecuteQuery(checkOrder, parameter).Rows.Count == 0 ? false : true;
                if (check)
                    break;
            }

            if (!check)
            {
                string deleteOldPicturePathQuery = "SELECT Picture FROM Product WHERE Id = @Id";
                var parameter = new Dictionary<string, object>
                {
                    { "@Id", id }
                };
                DataTable oldPictureResult = DataAccess.DataAccess.ExecuteQuery(deleteOldPicturePathQuery, parameter);
                string oldPicturePath = oldPictureResult.Rows[0]["Picture"].ToString();
                MyUtil.DeletePicture(oldPicturePath);
                string query = "Delete From Product Where id = @productId";
                parameter = new Dictionary<string, object>
                {
                    { "@productId", id }
                };
                DataAccess.DataAccess.ExecuteNonQuery(query, parameter);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
