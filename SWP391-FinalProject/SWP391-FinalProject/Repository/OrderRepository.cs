
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;
using Microsoft.Extensions.Primitives;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class OrderRepository
    {
        private readonly DBContext db;



        public OrderRepository()
        {
            db = new DBContext();
        }


        public string NewOrderId()
        {
            var query = "SELECT ID FROM `Order` Order By Id DESC LIMIT 1;";
            DataTable dataTable = DataAccess.DataAccess.ExecuteQuery(query);



            string lastId = dataTable.Rows[0]["Id"].ToString();

            // Split the prefix and number
            string prefix = lastId.Substring(0, 1); // Get the first character
            int number = int.Parse(lastId.Substring(1)); // Convert the rest to an integer

            // Increment the number by 1 and format the new ID with leading zeros
            int newNumber = number + 1;
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }

        public void Cancel(string id)
        {
            string query = "Update `Order` Set state_id = @state Where id = @Id";
            var parameter = new Dictionary<string, object> { { "@state", 4 }, { "@Id", id } };
            DataAccess.DataAccess.ExecuteNonQuery(query, parameter);

            //Update ProductItem Quantity
            OrderItemRepository orderItemRepo = new OrderItemRepository();
            var listOrderItem = orderItemRepo.GetOrderItemByOrderId(id);
            var proItemId = "";
            foreach (var orderItem in listOrderItem)
            {
                string updateQuery = @"
                UPDATE product_item 
                SET Quantity = Quantity + @Quantity
                WHERE Id = @ProductItemId";

                var updateParameters = new Dictionary<string, object>
                {
                    { "@ProductItemId", orderItem.ProductItemId },
                    { "@Quantity", orderItem.Quantity }
                };

                // Execute Update query
                DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
                proItemId = orderItem.ProductItemId;
            }

            //Update Product State
            ProductItemRepository proItemRepo = new ProductItemRepository();
            var proItem = proItemRepo.getProductItemByProductItemId(proItemId);
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProductState(proItem.Product.Id);

            //Update UserPoint
            OrderRepository orderRepo = new OrderRepository();
            var order = orderRepo.GetOrderByOrderId(id);
            UserRepository userRepo = new UserRepository();
            var user = userRepo.GetUserProfileByUserId(order.UserId);
            userRepo.UpdateUserPoint(user.Account.Username, order.StateId, order.UsePoint, order.EarnPoint);
        }

        public void InsertOrder(OrderModel Order, string username, decimal? TotalPrice, List<ProductItemModel> listProItem)
        {
            int currentHour = DateTime.Now.TimeOfDay.Hours;

            List<StaffShiftModel> staffShifts = (from s in db.StaffShifts
                                                 where s.Date == DateOnly.FromDateTime(DateTime.Today)
                                                 select new StaffShiftModel()
                                                 {
                                                     Id = s.Id,
                                                     Staff_Id = s.StaffId,
                                                 }).ToList();
            if (staffShifts == null || staffShifts.Count == 0)
            {
                int preDay = 1;
                do
                {
                    staffShifts = (from s in db.StaffShifts
                                   where s.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(preDay))
                                   select new StaffShiftModel()
                                   {
                                       Id = s.Id,
                                       Staff_Id = s.StaffId,
                                   }).ToList();
                    preDay--;
                } while (staffShifts == null);
            }
            if (currentHour >= 0 && currentHour <= 12)
            {
                Order.StaffShiftId = staffShifts[0].Id;
            }
            else if (currentHour > 12 && currentHour <= 18)
            {
                Order.StaffShiftId = staffShifts[1].Id;
            }
            else
            {
                staffShifts = (from s in db.StaffShifts
                               where s.Date == DateOnly.FromDateTime(DateTime.Today.AddDays(1))
                               select new StaffShiftModel()
                               {
                                   Staff_Id = s.StaffId,
                                   Id = s.Id
                               }).ToList();

                Order.StaffShiftId = staffShifts[0].Id;
            }

            string newId = NewOrderId();
            var newOrder = new Entities.Order()
            {
                Id = newId,
                UserId = Order.UserId,
                Address = Order.Addres,
                StateId = 1,
                Date = DateTime.Now,
                UsePoint = (decimal)(Order.UsePoint ?? Order.UsePoint),
                EarnPoint = TotalPrice / 1000,
                StaffShiftId = Order.StaffShiftId
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();

            InsertOrderItem(listProItem, newOrder.Id);
            UserRepository userRepo = new UserRepository();
            userRepo.UpdateUserPoint(username, newOrder.StateId, newOrder.UsePoint, newOrder.EarnPoint);
            ProductRepository proRepo = new ProductRepository();
            proRepo.UpdateProductState(listProItem.Select(p => p.Product.Id).First().ToString());
        }

        public void InsertOrderItem(List<ProductItemModel> listProItem, string orderID)
        {
            foreach (ProductItemModel item in listProItem)
            {
                // 1. Insert OrderItem into OrderItems table
                string insertQuery = @"
            INSERT INTO order_item (order_id, product_item_id, Quantity, Discount, Price)
            VALUES (@OrderId, @ProductItemId, @Quantity, @Discount, @Price)";

                var parameters = new Dictionary<string, object>
        {
            { "@OrderId", orderID },
            { "@ProductItemId", item.Id },
            { "@Quantity", item.CartQuantity },
            { "@Discount", item.Discount },
            { "@Price", item.SellingPrice ?? 0 }
        };

                // Execute Insert query
                DataAccess.DataAccess.ExecuteNonQuery(insertQuery, parameters);

                // 2. Update ProductItem Quantity based on the Order state (Assuming state "1" means the order was placed)
                string updateQuery = @"
            UPDATE product_item 
            SET Quantity = Quantity - @Quantity
            WHERE Id = @ProductItemId";

                var updateParameters = new Dictionary<string, object>
        {
            { "@ProductItemId", item.Id },
            { "@Quantity", item.CartQuantity }
        };

                // Execute Update query
                DataAccess.DataAccess.ExecuteNonQuery(updateQuery, updateParameters);
            }
        }



        public List<OrderModel> GetAllStaffOrder(string staffId)
        {
            // SQL query to get orders assigned to a specific staff member
            string query = @"
        SELECT 
            o.Id AS OrderId,
            o.user_id,
            o.Address,
            o.state_id,
            o.Date,
            o.use_point,
            o.earn_point,
            o.staff_shift_id,
            u.Name AS UserName,
            os.Name AS OrderStateName
        FROM `order` o
        INNER JOIN user u ON o.user_id = u.account_id
        INNER JOIN order_state os ON o.state_id = os.Id
        INNER JOIN staff_shift ss ON o.staff_shift_id = ss.Id
        WHERE ss.staff_id = @StaffId";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@StaffId", staffId }
    };

            // Execute the query and get the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            var orderList = new List<OrderModel>();

            foreach (DataRow row in resultTable.Rows)
            {
                var order = new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["user_id"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    UsePoint = Convert.ToDecimal(row["use_point"]),
                    EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                    StaffShiftId = row["staff_shift_id"].ToString(),

                    User = new UserModel { Name = row["UserName"].ToString() },

                    OrderState = new OrderState { Name = row["OrderStateName"].ToString() }
                };

                orderList.Add(order);
            }

            return orderList;
        }

        public List<OrderModel> GetAllOrder()
        {
            // SQL query to get all orders with related information
            string query = @"
        SELECT 
            o.Id AS OrderId,
            o.user_id,
            o.Address,
            o.state_id,
            o.Date,
            o.use_point,
            o.earn_point,
            o.staff_shift_id,
            ss.staff_id,
            s.Name AS StaffName,
            u.Name AS UserName,
            os.Name AS OrderStateName
        FROM `order` o
        INNER JOIN user u ON o.user_id = u.account_id
        INNER JOIN order_state os ON o.state_id = os.Id
        INNER JOIN staff_shift ss ON o.staff_shift_id = ss.Id
        INNER JOIN Staff s ON ss.staff_id = s.account_id
        ORDER By o.Date Desc";

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>());

            var orderList = new List<OrderModel>();

            foreach (DataRow row in resultTable.Rows)
            {
                var order = new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["user_id"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    UsePoint = Convert.ToDecimal(row["use_point"]),
                    EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                    StaffShiftId = row["staff_shift_id"].ToString(),
                    StaffName = row["StaffName"].ToString(),

                    User = new UserModel { Name = row["UserName"].ToString() },

                    OrderState = new OrderState { Name = row["OrderStateName"].ToString() }
                };

                orderList.Add(order);
            }

            return orderList;
        }
        public List<OrderModel> GetAllOrderWithKeyword(string keyword, DateTime? fromDate, DateTime? toDate, int? orderState)
        {
            // Base query to select orders with related information
            string query = @"
    SELECT 
        o.Id AS OrderId,
        o.user_id,
        o.Address,
        o.state_id,
        o.Date,
        o.use_point,
        o.earn_point,
        o.staff_shift_id,
        ss.staff_id,
        s.Name AS StaffName,
        u.Name AS UserName,
        os.Name AS OrderStateName
    FROM `order` o
    INNER JOIN user u ON o.user_id = u.account_id
    INNER JOIN order_state os ON o.state_id = os.Id
    INNER JOIN staff_shift ss ON o.staff_shift_id = ss.Id
    INNER JOIN Staff s ON ss.staff_id = s.account_id
    WHERE 1 = 1
    ";

            var parameters = new Dictionary<string, object>();

            // Keyword filtering
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += " AND (u.Name LIKE @keyword OR u.account_id LIKE @keyword)";
                parameters.Add("@keyword", $"%{keyword}%");
            }

            // Date range filtering
            if (fromDate.HasValue)
            {
                query += " AND o.Date >= @fromDate";
                parameters.Add("@fromDate", fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query += " AND o.Date <= @toDate";
                parameters.Add("@toDate", toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            // Order state filtering
            if (orderState.HasValue && orderState.Value > 0)
            {
                query += " AND o.state_id = @orderState";
                parameters.Add("@orderState", orderState.Value);
            }

            // Order by date descending
            query += " ORDER BY o.Date DESC;";

            // Execute the query and store results in a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            var orderList = new List<OrderModel>();

            // Process the DataTable to build the list of OrderModel objects
            foreach (DataRow row in resultTable.Rows)
            {
                var order = new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["user_id"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    UsePoint = Convert.ToDecimal(row["use_point"]),
                    EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                    StaffShiftId = row["staff_shift_id"].ToString(),
                    StaffName = row["StaffName"].ToString(),
                    User = new UserModel { Name = row["UserName"].ToString() },
                    OrderState = new OrderState { Name = row["OrderStateName"].ToString() }
                };

                orderList.Add(order);
            }

            return orderList;
        }


        public List<OrderModel> GetAllStaffOrderWithKeyword(string staffId, string keyword, DateTime? fromDate, DateTime? toDate, int? orderState)
        {
            // SQL query to get orders assigned to a specific staff member with dynamic filtering
            string query = @"
    SELECT 
        o.Id AS OrderId,
        o.user_id,
        o.Address,
        o.state_id,
        o.Date,
        o.use_point,
        o.earn_point,
        o.staff_shift_id,
        u.Name AS UserName,
        os.Name AS OrderStateName
    FROM `order` o
    INNER JOIN user u ON o.user_id = u.account_id
    INNER JOIN order_state os ON o.state_id = os.Id
    INNER JOIN staff_shift ss ON o.staff_shift_id = ss.Id
    WHERE ss.staff_id = @StaffId";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@StaffId", staffId }
    };

            // Keyword filtering
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += " AND (u.Name LIKE @keyword OR u.account_id LIKE @keyword)";
                parameters.Add("@keyword", $"%{keyword}%");
            }

            // Date range filtering
            if (fromDate.HasValue)
            {
                query += " AND o.Date >= @fromDate";
                parameters.Add("@fromDate", fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query += " AND o.Date <= @toDate";
                parameters.Add("@toDate", toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            // Order state filtering
            if (orderState.HasValue && orderState.Value > 0)
            {
                query += " AND o.state_id = @orderState";
                parameters.Add("@orderState", orderState.Value);
            }

            // Order by date descending
            query += " ORDER BY o.Date DESC;";

            // Execute the query and get the results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            var orderList = new List<OrderModel>();

            foreach (DataRow row in resultTable.Rows)
            {
                var order = new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["user_id"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    UsePoint = Convert.ToDecimal(row["use_point"]),
                    EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                    StaffShiftId = row["staff_shift_id"].ToString(),

                    User = new UserModel { Name = row["UserName"].ToString() },
                    OrderState = new OrderState { Name = row["OrderStateName"].ToString() }
                };

                orderList.Add(order);
            }

            return orderList;
        }

        public OrderModel GetOrderByOrderId(string orderId)
        {
            // SQL query to get order details by orderId
            string query = @"
        SELECT 
            o.Id,
            o.user_id,
            o.Address,
            o.state_id,
            os.Name AS OrderStateName,
            o.use_point ,
            o.earn_point,
            o.Date
        FROM `order` o
        INNER JOIN order_state os ON o.state_id = os.Id
        WHERE o.Id = @OrderId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@OrderId", orderId }
    };

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count > 0)
            {
                // Map the result row to OrderModel
                DataRow row = resultTable.Rows[0];

                var order = new OrderModel
                {
                    Id = row["Id"].ToString(),
                    UserId = row["user_id"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = Convert.ToInt32(row["state_id"]),
                    OrderState = new OrderState
                    {
                        Name = row["OrderStateName"].ToString()
                    },
                    UsePoint = Convert.ToDecimal(row["use_point"]),
                    EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                    Date = Convert.ToDateTime(row["Date"])
                };

                return order;
            }

            return null;
        }


        public List<OrderModel> GetOrderByUserId(string UserId)
        {
            string query = "Select o.id as OrderId, o.user_id as UserId, o.address as Address, " +
               "o.state_id as StateId, os.name as StateName, o.use_point as UsePoint, " +
               "o.earn_point as EarnPoint, o.date as OrderDate " +
               "From `Order` o " +
               "Inner join Order_State os on o.state_id = os.id " +
               "Where o.user_id = @UserId Order by o.date DESC";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", UserId }
            };
            DataTable orderTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);
            List<OrderModel> orderList = new List<OrderModel>();
            foreach (DataRow row in orderTable.Rows)
            {
                orderList.Add(new OrderModel
                {
                    Id = row["OrderId"].ToString(),
                    UserId = row["UserId"].ToString(),
                    Addres = row["Address"].ToString(),
                    StateId = int.Parse(row["StateId"].ToString()),
                    OrderState = new OrderState { Name = row["StateName"].ToString() },
                    UsePoint = decimal.Parse(row["UsePoint"].ToString()),
                    EarnPoint = decimal.Parse(row["EarnPoint"].ToString()),
                    Date = DateTime.Parse(row["OrderDate"].ToString())
                });
            }

            OrderItemRepository orderItemRepo = new OrderItemRepository();
            List<OrderItemModel> orderItemList = new List<OrderItemModel>();

            foreach (var order in orderList)
            {
                orderItemList = orderItemRepo.GetOrderItemByOrderId(order.Id);
                order.TotalPrice = 0;
                order.TotalPrice = GetTotalPrice(orderItemList, order);
            }
            return orderList;
        }

        public List<OrderState> GetAllOrderState()
        {
            // Define the SQL query to select all order states
            string query = "SELECT Id, Name FROM order_state";

            // Execute the query and map results to OrderState model
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);
            var listOrderState = new List<OrderState>();

            foreach (DataRow row in resultTable.Rows)
            {
                var orderState = new OrderState
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = row["Name"].ToString()
                };

                // Add the order state to the list
                listOrderState.Add(orderState);
            }

            return listOrderState;
        }



        public void UpdateOrderState(int orderStateId, string orderId)
        {
            // SQL query to update the state of the order
            string query = @"
        UPDATE `order` 
        SET state_id = @OrderStateId 
        WHERE Id = @OrderId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@OrderStateId", orderStateId },
        { "@OrderId", orderId }
    };

            // Execute the query
            DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }


        public List<OrderItemModel> GetOrderItemByOrderId(string orderId)
        {
            ProductRepository proRepo = new ProductRepository();

            // SQL query to fetch order items with necessary joins and conditions
            string query = @"
        SELECT 
            p.Picture,
            p.Name,
            oi.Quantity,
            oi.Price,
            pi.Discount,
            pi.Id AS ProductItemId
        FROM order_item oi
        INNER JOIN product_item pi ON oi.product_item_id = pi.Id
        INNER JOIN product p ON pi.product_id = p.Id
        INNER JOIN `order` o ON oi.order_id = o.Id
        WHERE o.Id = @OrderId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@OrderId", orderId }
    };

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            var orderItems = new List<OrderItemModel>();

            // Map each row in DataTable to OrderItemModel
            foreach (DataRow row in resultTable.Rows)
            {
                var productItemId = row["ProductItemId"].ToString();
                var discount = row["Discount"] != DBNull.Value ? Convert.ToDecimal(row["Discount"]) : 0;

                var orderItem = new OrderItemModel
                {
                    Product = new ProductModel
                    {
                        Picture = row["Picture"].ToString(),
                        Name = row["Name"].ToString()
                    },
                    Ram = proRepo.GetProductVariationOption(productItemId, "Ram"),
                    Storage = proRepo.GetProductVariationOption(productItemId, "Storage"),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Price = Convert.ToDecimal(row["Price"]),
                    Discount = discount,

                    // Calculate TotalPrice with discount
                    TotalPrice = (Convert.ToInt32(row["Quantity"]) * Convert.ToDecimal(row["Price"])) * (1 - (discount / 100))
                };

                orderItems.Add(orderItem);
            }

            return orderItems;
        }



        public decimal? GetTotalPrice(List<OrderItemModel> orderItems, OrderModel order)
        {
            foreach (var item in orderItems)
            {
                // If Discount is null, it will default to 0
                decimal discount = item.Discount ?? 0;

                // Calculate total price for each item and add it to order.TotalPrice
                order.TotalPrice += (item.Price * item.Quantity) * (1 - (discount / 100));
            }
            return order.TotalPrice;
        }


        public OrderModel GetOrderDetail(string orderId)
        {
            // Define the SQL query to fetch order details
            string query = @"
        SELECT 
            earn_point,
            use_point
        FROM `order`
        WHERE Id = @OrderId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@OrderId", orderId }
    };

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count == 0)
                throw new ArgumentException($"Order with ID '{orderId}' not found.");

            // Map DataTable result to OrderModel
            DataRow row = resultTable.Rows[0];
            var order = new OrderModel
            {
                EarnPoint = row["earn_point"] != DBNull.Value ? Convert.ToDecimal(row["earn_point"]) : 0,
                UsePoint = row["use_point"] != DBNull.Value ? Convert.ToDecimal(row["use_point"]) : 0,
                TotalPrice = 0 // Default value
            };

            return order;
        }

    }
}
