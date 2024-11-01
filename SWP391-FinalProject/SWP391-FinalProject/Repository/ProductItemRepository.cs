using SWP391_FinalProject.Controllers;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class ProductItemRepository
    {
        private readonly DBContext db;
        public ProductItemRepository()
        {
            db = new DBContext();
        }

        public string getNewProductItemID()
        {
            // Query to get the latest ID in descending order
            string query = "SELECT Id FROM SWP391.Product_Item ORDER BY Id DESC LIMIT 1";
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if result is empty, meaning no records are found
            string lastId = result.Rows.Count > 0 ? result.Rows[0]["Id"].ToString() : null;

            if (lastId == null)
            {
                return "Pi000001";
            }

            // Extract the prefix and number from the last ID
            string prefix = lastId.Substring(0, 2); // Get the first two characters
            int number = int.Parse(lastId.Substring(2)); // Parse the numeric part of the ID

            // Increment the number by 1
            int newNumber = number + 1;

            // Format the new ID with the incremented number
            string newId = $"{prefix}{newNumber:D6}";

            return newId;
        }


        public int getNewProductConfigurationID()
        {
            // Query to get the highest Id in the ProductConfigurations table
            string query = "SELECT MAX(Id) FROM SWP391.Product_Configuration";
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if result is empty or NULL, meaning no records are found
            int lastId = result.Rows[0][0] != DBNull.Value ? Convert.ToInt32(result.Rows[0][0]) : 0;

            // Increment the last ID by 1 to get the new ID
            int newId = lastId + 1;

            return newId;
        }


        public string GetVariationId(string name)
        {
            // Query to get the Id of the Variation based on its Name
            string query = "SELECT Id FROM SWP391.Variation WHERE Name = @Name LIMIT 1";

            // Set up the parameter for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Name", name }
    };

            // Execute the query and retrieve the result
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Return the Id if a result is found; otherwise, return null
            return result.Rows.Count > 0 ? result.Rows[0]["Id"].ToString() : null;
        }


        public string GetVariationOptionId(string value, string variationId)
        {
            // Query to get the Id of the VariationOption based on its Value and VariationId
            string query = "SELECT Id FROM SWP391.Variation_Option WHERE Value = @Value AND variation_id = @VariationId LIMIT 1";

            // Set up the parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Value", value },
        { "@VariationId", variationId }
    };

            // Execute the query and retrieve the result
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Return the Id if a result is found; otherwise, return null
            return result.Rows.Count > 0 ? result.Rows[0]["Id"].ToString() : null;
        }


        public bool CheckExistVariation(string productId, string variationOpId1, string variationOpId2)
        {
            // Query to check if the product has the first variation option
            string query1 = @"
        SELECT 1 
        FROM SWP391.Product p
        JOIN SWP391.Product_Item pi ON p.Id = pi.product_id
        JOIN SWP391.Product_Configuration pc ON pi.Id = pc.product_item_id
        WHERE p.Id = @ProductId AND pc.variation_option_id = @VariationOpId1
        LIMIT 1";

            // Query to check if the product has the second variation option
            string query2 = @"
        SELECT 1 
        FROM SWP391.Product p
        JOIN SWP391.Product_Item pi ON p.Id = pi.product_id
        JOIN SWP391.Product_Configuration pc ON pi.Id = pc.product_item_id
        WHERE p.Id = @ProductId AND pc.variation_option_id = @VariationOpId2
        LIMIT 1";

            // Set up the parameters for each query
            var parameters1 = new Dictionary<string, object>
    {
        { "@ProductId", productId },
        { "@VariationOpId1", variationOpId1 }
    };

            var parameters2 = new Dictionary<string, object>
    {
        { "@ProductId", productId },
        { "@VariationOpId2", variationOpId2 }
    };

            // Execute both queries
            DataTable result1 = DataAccess.DataAccess.ExecuteQuery(query1, parameters1);
            DataTable result2 = DataAccess.DataAccess.ExecuteQuery(query2, parameters2);

            // Return true if both queries return results, meaning both variation options exist
            return result1.Rows.Count > 0 && result2.Rows.Count > 0;
        }


        public ProductItemModel getProductItemByProductItemId(string proItemId)
        {
            // Define the SQL query with JOIN
            string query = @"
        SELECT pi.Id, pi.SellingPrice, pi.Quantity, pi.Discount, 
               p.Name AS ProductName, p.Picture, p.Description, p.Id AS ProductId
        FROM ProductItems pi
        JOIN Products p ON pi.ProductId = p.Id
        WHERE pi.Id = @ProductItemId";

            // Define the parameter for the query
            var parameters = new Dictionary<string, object>
    {
        { "@ProductItemId", proItemId }
    };

            // Execute the query
            DataTable result = DataAccess.ExecuteQuery(query, parameters);

            // Check if any rows are returned
            if (result.Rows.Count == 0)
            {
                return null; // No matching record found
            }

            // Map the result to ProductItemModel
            DataRow row = result.Rows[0];
            var productItemModel = new ProductItemModel
            {
                Id = row["Id"].ToString(),
                SellingPrice = Convert.ToDecimal(row["SellingPrice"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Discount = Convert.ToDecimal(row["Discount"]),
                Product = new ProductModel
                {
                    Name = row["ProductName"].ToString(),
                    Picture = row["Picture"].ToString(),
                    Description = row["Description"].ToString(),
                    Id = row["ProductId"].ToString()
                }
            };

            return productItemModel;
        }


        public void AddProductConfiguration(ProductItemModel model)
        {
            // Get variation option IDs
            var variationOptionId1 = GetVariationOptionId(model.Ram, GetVariationId("Ram"));
            var variationOptionId2 = GetVariationOptionId(model.Storage, GetVariationId("Storage"));

            // Check if the variation already exists
            if (CheckExistVariation(model.ProductId, variationOptionId1, variationOptionId2))
            {
                // Query to delete the existing ProductItem if the variation exists
                string deleteProductItemQuery = "DELETE FROM SWP391.Product_Item WHERE Id = @ProductItemId";
                var deleteParams = new Dictionary<string, object> { { "@ProductItemId", model.Id } };
                DataAccess.DataAccess.ExecuteNonQuery(deleteProductItemQuery, deleteParams);

                return;
            }
            else
            {
                // Generate a new ID for ProductConfiguration
                var newId = getNewProductConfigurationID();

                // Insert first configuration
                string insertConfigQuery1 = @"
            INSERT INTO SWP391.Product_Configuration (Id, product_item_id, variation_option_id)
            VALUES (@Id, @ProductItemId, @VariationOptionId)";

                var insertParams1 = new Dictionary<string, object>
        {
            { "@Id", newId },
            { "@ProductItemId", model.Id },
            { "@VariationOptionId", variationOptionId1 }
        };
                DataAccess.DataAccess.ExecuteNonQuery(insertConfigQuery1, insertParams1);

                // Insert second configuration
                newId++;  // Increment ID for second configuration
                var insertParams2 = new Dictionary<string, object>
        {
            { "@Id", newId },
            { "@ProductItemId", model.Id },
            { "@VariationOptionId", variationOptionId2 }
        };
                DataAccess.DataAccess.ExecuteNonQuery(insertConfigQuery1, insertParams2);

                // Insert quantity to ProductLog
                InsertQuantityToProductLog(model.Quantity, model.Id);
            }
        }


        public void InsertQuantityToProductLog(int quantity, string proItemId)
        {
            // Generate unique IDs for QuantityLog and ProductLog
            string quanLogId;
            string proLogId;

            do
            {
                quanLogId = StaffManController.GenerateRandomString(8);
                string checkQuantityLogIdQuery = "SELECT COUNT(1) FROM SWP391.Quantity_Log WHERE Id = @Id";
                var checkQuantityLogIdParams = new Dictionary<string, object> { { "@Id", quanLogId } };
                if ((long)DataAccess.DataAccess.ExecuteQuery(checkQuantityLogIdQuery, checkQuantityLogIdParams).Rows[0][0] == 0)
                    break;
            } while (true);

            do
            {
                proLogId = StaffManController.GenerateRandomString(8);
                string checkProductLogIdQuery = "SELECT COUNT(1) FROM SWP391.Product_Log WHERE Id = @Id";
                var checkProductLogIdParams = new Dictionary<string, object> { { "@Id", proLogId } };
                if ((int)DataAccess.DataAccess.ExecuteQuery(checkProductLogIdQuery, checkProductLogIdParams).Rows[0][0] == 0)
                    break;
            } while (true);

            // Insert into QuantityLogs
            string insertQuantityLogQuery = @"
        INSERT INTO SWP391.Quantity_Log (Id, product_item_id, old_quantity, new_quantity, change_timestamp)
        VALUES (@Id, @ProductItemId, @OldQuantity, @NewQuantity, @ChangeTimestamp);";

            var quantityLogParams = new Dictionary<string, object>
    {
        { "@Id", quanLogId },
        { "@ProductItemId", proItemId },
        { "@OldQuantity", 0 },  // Assuming OldQuantity is 0 as per the original code
        { "@NewQuantity", quantity },
        { "@ChangeTimestamp", DateTime.UtcNow }
    };

            DataAccess.DataAccess.ExecuteNonQuery(insertQuantityLogQuery, quantityLogParams);

            // Insert into ProductLogs
            string insertProductLogQuery = @"
        INSERT INTO SWP391.Product_Log (Id, quantity_log_id, change_reason_id)
        VALUES (@Id, @QuantityLogId, @ChangeReasonId);";

            var productLogParams = new Dictionary<string, object>
    {
        { "@Id", proLogId },
        { "@QuantityLogId", quanLogId },
        { "@ChangeReasonId", "1" }  // Hardcoded as per the original code
    };

            DataAccess.DataAccess.ExecuteNonQuery(insertProductLogQuery, productLogParams);
        }



        public void AddProductItem(ProductItemModel model)
        {
            // Generate a new ID for ProductItem
            model.Id = getNewProductItemID();
            model.Discount = 0;

            // Insert new ProductItem into the database
            string insertProductItemQuery = @"
        INSERT INTO SWP391.Product_Item (Id, product_id, Quantity, import_price, selling_price, Discount)
        VALUES (@Id, @ProductId, @Quantity, @ImportPrice, @SellingPrice, @Discount);";

            var productItemParameters = new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@ProductId", model.ProductId },
        { "@Quantity", model.Quantity },
        { "@ImportPrice", model.ImportPrice },
        { "@SellingPrice", model.SellingPrice },
        { "@Discount", model.Discount }
    };

            DataAccess.DataAccess.ExecuteNonQuery(insertProductItemQuery, productItemParameters);

            // Update the Product's StateId if it's not already equal to 2
            string updateProductStateQuery = @"
        UPDATE  SWP391.Product
        SET state_id = 1
        WHERE Id = @ProductId AND state_id != 2;";

            var productStateParameters = new Dictionary<string, object>
    {
        { "@ProductId", model.ProductId }
    };

            DataAccess.DataAccess.ExecuteNonQuery(updateProductStateQuery, productStateParameters);

            // Call AddProductConfiguration to add product configurations
            AddProductConfiguration(model);
        }


        public void UpdateProductItemQuantityByOrderStateId(string proItemId, int quantity, int orderStateId)
        {
            // Check if the product item exists
            string checkQuery = "SELECT COUNT(1) FROM SWP391.Product_Item WHERE Id = @Id";
            var checkParameters = new Dictionary<string, object> { { "@Id", proItemId } };
            DataTable checkResult = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (checkResult.Rows[0][0].ToString() == "0")
            {
                Console.WriteLine("Product item not found!");
                return;
            }

            // Determine the SQL update query based on OrderStateId
            string updateQuery;
            if (orderStateId == 1)
            {
                updateQuery = @"
            UPDATE SWP391.Product_Item
            SET Quantity = Quantity - @Quantity
            WHERE Id = @Id;";
            }
            else if (orderStateId == 3)
            {
                updateQuery = @"
            UPDATE SWP391.Product_Item
            SET Quantity = Quantity + @Quantity
            WHERE Id = @Id;";
            }
            else
            {
                Console.WriteLine("Invalid OrderStateId provided!");
                return;
            }

            // Define parameters for the update query
            var updateParameters = new Dictionary<string, object>
    {
        { "@Id", proItemId },
        { "@Quantity", quantity }
    };

            // Execute the update query
            DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
        }


        public void EditProductItem(ProductItemModel model)
        {
            // Check if the product item exists
            string checkQuery = "SELECT COUNT(1) FROM SWP391.Product_Item WHERE Id = @Id";
            var checkParameters = new Dictionary<string, object> { { "@Id", model.Id } };
            DataTable checkResult = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (checkResult.Rows[0][0].ToString() == "0")
            {
                Console.WriteLine("Product item not found!");
                return;
            }

            // Update the ProductItem's SellingPrice and Discount
            string updateQuery = @"
        UPDATE SWP391.Product_Item
        SET selling_price = @SellingPrice,
            Discount = @Discount
        WHERE Id = @Id;";

            // Define parameters for the update query
            var updateParameters = new Dictionary<string, object>
    {
        { "@Id", model.Id },
        { "@SellingPrice", model.SellingPrice },
        { "@Discount", model.Discount }
    };

            // Execute the update query
            DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
        }


        public void Delete(string id)
        {
            // Check if the product item exists
            string checkQuery = "SELECT COUNT(1) FROM SWP391.Product_Item WHERE Id = @Id";
            var checkParameters = new Dictionary<string, object> { { "@Id", id } };
            DataTable checkResult = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (checkResult.Rows[0][0].ToString() == "0")
            {
                Console.WriteLine("Product item not found!");
                return;
            }

            // Fetch the ProductId associated with the ProductItem to update its state later
            string getProductIdQuery = "SELECT product_id FROM SWP391.Product_Item WHERE Id = @Id";
            DataTable productItemTable = DataAccess.DataAccess.ExecuteQuery(getProductIdQuery, checkParameters);
            string productId = productItemTable.Rows[0]["product_id"].ToString();

            // Delete related entries from ProductConfigurations
            string deleteProductConfigurationsQuery = "DELETE FROM SWP391.Product_Configuration WHERE product_item_id = @Id";
            DataAccess.DataAccess.ExecuteNonQuery(deleteProductConfigurationsQuery, checkParameters);

            // Delete the ProductItem
            string deleteProductItemQuery = "DELETE FROM SWP391.Product_Item WHERE Id = @Id";
            DataAccess.DataAccess.ExecuteNonQuery(deleteProductItemQuery, checkParameters);

            // Update product state
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProductState(productId);
        }




        public decimal? GetPriceByProductItemId(string productItemId)
        {
            // Query to get SellingPrice and Discount for the specified ProductItem
            string query = @"
        SELECT selling_price, Discount
        FROM SWP391.Product_Item
        WHERE Id = @ProductItemId;";

            // Define the parameter for the query
            var parameters = new Dictionary<string, object>
    {
        { "@ProductItemId", productItemId }
    };

            // Execute the query
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if a result was found
            if (result.Rows.Count == 0)
            {
                Console.WriteLine("Product item not found!");
                return null;
            }

            // Get the SellingPrice and Discount values
            decimal sellingPrice = Convert.ToDecimal(result.Rows[0]["selling_price"]);
            decimal discount = Convert.ToDecimal(result.Rows[0]["Discount"]) / 100;

            // Calculate the final price after discount
            return ProductRepository.CalculatePriceAfterDiscount(sellingPrice, discount);
        }


        public void Import(string id, int quantity)
        {
            // Check if the product item exists
            string checkQuery = "SELECT COUNT(1) FROM SWP391.Product_Item WHERE Id = @Id";
            var checkParameters = new Dictionary<string, object> { { "@Id", id } };
            DataTable checkResult = DataAccess.DataAccess.ExecuteQuery(checkQuery, checkParameters);

            if (checkResult.Rows[0][0].ToString() == "0")
            {
                Console.WriteLine("Product item not found!");
                return;
            }

            // Update the ProductItem's Quantity
            string updateQuantityQuery = @"
        UPDATE SWP391.Product_Item
        SET Quantity = Quantity + @Quantity
        WHERE Id = @Id;";

            var updateParameters = new Dictionary<string, object>
    {
        { "@Id", id },
        { "@Quantity", quantity }
    };

            // Execute the update query
            DataAccess.DataAccess.ExecuteNonQuery(updateQuantityQuery, updateParameters);

            // Get the ProductId associated with the ProductItem for updating its state
            string getProductIdQuery = "SELECT product_id FROM SWP391.Product_Item WHERE Id = @Id";
            DataTable productItemTable = DataAccess.DataAccess.ExecuteQuery(getProductIdQuery, checkParameters);
            string productId = productItemTable.Rows[0]["product_id"].ToString();

            // Update product state
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProductState(productId);
        }

    }
}
