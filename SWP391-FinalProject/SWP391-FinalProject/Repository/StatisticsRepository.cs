using Microsoft.EntityFrameworkCore;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class StatisticsRepository
    {
        public StatisticsRepository()
        {

        }
        public List<dynamic> GetSellingPrice(int year)
        {
            string query = @"
        SELECT MONTH(o.Date) AS Month,
               SUM(oi.Quantity * oi.price * (1 - oi.Discount / 100)) AS TotalSellingPrice
        FROM `Order` o
        JOIN Order_Item oi ON o.Id = oi.Order_Id
        WHERE o.State_Id = 2 AND YEAR(o.Date) = @year
        GROUP BY MONTH(o.Date)
        ORDER BY MONTH(o.Date);";

            var parameters = new Dictionary<string, object>
    {
        { "@year", year }
    };

            // Execute the query and get results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            var sellingPrices = new List<dynamic>();

            foreach (DataRow row in resultTable.Rows)
            {
                sellingPrices.Add(new
                {
                    Month = (int)row["Month"],
                    TotalSellingPrice = (decimal)row["TotalSellingPrice"]
                });
            }

            return sellingPrices;
        }


        public List<dynamic> GetImportPrice(int year)
        {
            // SQL query to get the import price based on the specified year
            string query = @"
        SELECT 
            MONTH(ql.change_timestamp) AS change_month, 
            SUM((ql.new_quantity - ql.old_quantity) * pi.import_price) AS total_import_price
        FROM 
            Quantity_Log ql
        JOIN 
            Product_Item pi ON ql.product_item_id = pi.id
        WHERE 
            ql.new_quantity > ql.old_quantity AND YEAR(ql.change_timestamp) = @year
        GROUP BY 
            change_month
        ORDER BY 
            change_month;";

            // Parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@year", year }
    };

            // Execute the query to get the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            var importPrices = new List<dynamic>();

            // Process the results into a list of dynamic objects
            foreach (DataRow row in resultTable.Rows)
            {
                importPrices.Add(new
                {
                    ChangeMonth = (int)row["change_month"], // Ensure proper casting
                    TotalImportPrice = (decimal)(row["total_import_price"] != DBNull.Value ? row["total_import_price"] : 0) // Handling NULLs
                });
            }

            return importPrices;
        }


        public List<dynamic> GetOrderStat()
        {
            // SQL query to get order statistics grouped by year and month
            string query = @"
        SELECT 
            YEAR(o.date) AS Year,
            MONTH(o.date) AS Month,
            SUM(oi.quantity) AS TotalOrder
        FROM 
            `Order` o
        JOIN 
            Order_Item oi ON o.id = oi.order_id
        WHERE 
            o.state_id = 2 -- Filter for approved orders
        GROUP BY 
            Year, Month
        ORDER BY 
            Year, Month;";

            // Execute the query to get the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var orderStats = new List<dynamic>();

            // Process the results into a list of dynamic objects
            foreach (DataRow row in resultTable.Rows)
            {
                orderStats.Add(new
                {
                    Year = (int)row["Year"],
                    Month = (int)row["Month"],
                    TotalOrder = row["TotalOrder"] != DBNull.Value ? Convert.ToInt32(Convert.ToDecimal(row["TotalOrder"])) : 0 // Handle conversion properly
                });
            }

            return orderStats;
        }


        public dynamic GetBestSellingProducts()
        {
            // SQL query to get the best-selling products
            string query = @"
        SELECT 
            p.name AS ProductName,
            COUNT(*) AS Count
        FROM 
            `Order` o
        JOIN 
            Order_Item oi ON o.id = oi.order_id
        JOIN 
            Product_Item pi ON oi.product_item_id = pi.id
        JOIN 
            Product p ON pi.product_id = p.id
        WHERE 
            o.state_id = 2
        GROUP BY 
            p.name
        ORDER BY 
            Count DESC
        LIMIT 5;";

            // Execute the query to get the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var bestSellingProducts = new List<dynamic>();

            // Process the results into a list of dynamic objects
            foreach (DataRow row in resultTable.Rows)
            {
                bestSellingProducts.Add(new
                {
                    ProductName = (string)row["ProductName"],
                    Count = Convert.ToInt32(row["Count"]) // Convert to int from long
                });
            }

            return bestSellingProducts;
        }


        public dynamic GetMostProfitableProducts()
        {
            // SQL query to get the most profitable products
            string query = @"
                   SELECT 
                p.Name AS ProductName,
                SUM(oi.Quantity * oi.Price * (1 - oi.Discount / 100)) AS TotalRevenue,
                SUM(oi.Quantity * pi.import_price) AS TotalImportCost,
                SUM(oi.Quantity * oi.Price * (1 - oi.Discount / 100)) - SUM(oi.Quantity * pi.import_price) AS Profit
            FROM 
                `Order` o
            JOIN 
                Order_Item oi ON o.Id = oi.Order_Id
            JOIN 
                Product_Item pi ON oi.Product_Item_Id = pi.Id
            JOIN 
                Product p ON pi.Product_Id = p.Id
            WHERE 
                o.State_Id = 2
            GROUP BY 
                p.Name
            ORDER BY 
                Profit DESC
            LIMIT 5;";

            // Execute the query and return results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var topProfitableProducts = new List<dynamic>();

            foreach (DataRow row in resultTable.Rows)
            {
                topProfitableProducts.Add(new
                {
                    ProductName = (string)row["ProductName"],
                    Profit = (decimal)(row["Profit"] != DBNull.Value ? row["Profit"] : 0) // Handle NULLs
                });
            }

            return topProfitableProducts;
        }


        public dynamic GetBestSellingBrands()
        {
            // SQL query to get the best-selling brands (categories)
            string query = @"
    SELECT 
        c.Name AS CategoryName,
        COUNT(pi.Id) AS ProductCount
    FROM 
        `Order` o
    JOIN 
        Order_Item oi ON o.Id = oi.Order_Id
    JOIN 
        Product_Item pi ON oi.Product_Item_Id = pi.Id
    JOIN 
        Product p ON pi.Product_Id = p.Id
    JOIN 
        Category c ON p.Category_Id = c.Id
    WHERE 
        o.State_Id = 2
    GROUP BY 
        c.Name
    ORDER BY 
        ProductCount DESC
    LIMIT 5;";

            // Execute the query and return results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var topCategories = new List<dynamic>();

            foreach (DataRow row in resultTable.Rows)
            {
                topCategories.Add(new
                {
                    CategoryName = (string)row["CategoryName"],
                    ProductCount = Convert.ToInt32(row["ProductCount"]) // Handle NULLs
                });
            }

            return topCategories;
        }


        public dynamic GetMostSpendingCustomers()
        {
            // SQL query to get the most spending customers
            string query = @"
    SELECT 
        u.Name AS UserName,
        SUM(oi.Quantity * oi.Price * (1 - oi.Discount / 100)) AS MoneyUsed
    FROM 
        `Order` o
    JOIN 
        Order_Item oi ON o.Id = oi.Order_Id
    JOIN 
        User u ON o.User_Id = u.Account_Id
    WHERE 
        o.State_Id = 2
    GROUP BY 
        u.Name
    ORDER BY 
        MoneyUsed DESC
    LIMIT 5;";

            // Execute the query and return results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var topSpendingUsers = new List<dynamic>();

            foreach (DataRow row in resultTable.Rows)
            {
                topSpendingUsers.Add(new
                {
                    UserName = (string)row["UserName"],
                    MoneyUsed = (decimal)(row["MoneyUsed"] != DBNull.Value ? row["MoneyUsed"] : 0) // Handle NULLs
                });
            }

            return topSpendingUsers;
        }


    }
}
