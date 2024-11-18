using Microsoft.AspNetCore.Mvc;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SWP391_FinalProject.Helpers;
using System.Data;

namespace SWP391_FinalProject.Repository
{
    public class StaffRepository
    {
        private readonly DBContext db;

        public StaffRepository()
        {
            db = new DBContext();
        }

        public string GetNewId()
        {
            // Query to get the latest ID in descending order
            string query = "SELECT Id FROM Staff_Shift ORDER BY Id DESC LIMIT 1";
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if result is empty, meaning no records are found
            string lastId = result.Rows.Count > 0 ? result.Rows[0]["Id"].ToString() : null;

            if (lastId == null)
            {
                return "S0000001";
            }

            // Extract the prefix and number from the last ID
            string prefix = lastId.Substring(0, 1); // Get the first two characters
            int number = int.Parse(lastId.Substring(1)); // Parse the numeric part of the ID

            // Increment the number by 1
            int newNumber = number + 1;

            // Format the new ID with the incremented number
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
        }
        public dynamic GetStaffSchedule(string staffId)
        {
            // SQL query to retrieve the schedule for the specified staff member
            string query = @"
    SELECT 
        Date,
        Shift
    FROM 
        Staff_Shift
    WHERE 
        Staff_Id = @StaffId;";

            // Create a dictionary to pass parameters
            var parameters = new Dictionary<string, object>
    {
        { "@StaffId", staffId }
    };

            // Execute the query and get the result as a DataTable
            DataTable scheduleTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Prepare the list to store the schedule results
            var schedule = new List<ShiftSchdeduleModel>();

            // Convert each DataRow to a ShiftScheduleModel object
            foreach (DataRow row in scheduleTable.Rows)
            {
                schedule.Add(new ShiftSchdeduleModel
                {
                    Date = DateOnly.FromDateTime((DateTime)row["Date"]),  // Convert DateTime to DateOnly
                    Shift = (string)row["Shift"]
                });

            }

            return schedule;
        }

        public List<StaffModel> GetAllStaff()
        {
            // SQL query to get all required data in one call
            string query = @"
        SELECT 
            s.account_Id AS Id,
            s.Name AS Name,
            a.Email,
            CASE WHEN a.Is_Active = 1 THEN 'Available' ELSE 'Unavailable' END AS Status,
            IFNULL(hours_worked.HoursWorked, 0) AS TotalHourWorked,
            IFNULL(order_count.OrderCount, 0) AS TotalOrders,
            IFNULL(order_count.OrderCount / (hours_worked.HoursWorked / 5), 0) AS AvgOrder,
            s.Salary AS Salary
        FROM 
            Staff s
        JOIN 
            Account a ON s.Account_Id = a.Id
        LEFT JOIN 
            (SELECT 
                 staff_id, 
                 COUNT(*) * 5 AS HoursWorked
             FROM 
                 staff_shift
             WHERE 
                 Date <= CURDATE()
             GROUP BY 
                 staff_id) AS hours_worked ON s.Account_Id = hours_worked.staff_id
        LEFT JOIN 
            (SELECT 
                 ss.staff_id, 
                 COUNT(o.Id) AS OrderCount
             FROM 
                 staff_shift ss
             JOIN 
                 `Order` o ON ss.Id = o.staff_shift_id
             GROUP BY 
                 ss.staff_id) AS order_count ON s.account_id = order_count.staff_id
        WHERE 
            a.Role_Id = 'Role0002';";

            // Execute query and get result
            DataTable staffData = DataAccess.DataAccess.ExecuteQuery(query);

            // Convert DataTable rows to StaffModel objects
            var result = staffData.AsEnumerable().Select(row => new StaffModel
            {
                Id = row["Id"].ToString(),
                Name = row["Name"].ToString(),
                Account = new AccountModel
                {
                    Status = row["Status"].ToString(),
                    Email = row["Email"].ToString()
                },
                TotalHourWorked = Convert.ToInt32(row["TotalHourWorked"]),
                TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                AvgOrder = Convert.ToDouble(row["AvgOrder"]),
                Salary = Convert.ToDouble(row["Salary"])
            }).ToList();

            return result;
        }
        public List<StaffModel> GetAllStaffByKeyword(string keyword)
        {
            // SQL query to retrieve staff details with filtering by keyword
            string staffQuery = @"
    SELECT 
        s.account_Id AS Id,
        s.Name AS Name,
        CASE WHEN a.Is_Active = 1 THEN 'Available' ELSE 'Unavailable' END AS Status,
        IFNULL(hours_worked.HoursWorked, 0) AS TotalHourWorked,
        IFNULL(order_count.OrderCount, 0) AS TotalOrders,
        IFNULL(order_count.OrderCount / (hours_worked.HoursWorked / 5), 0) AS AvgOrder,
        s.Salary AS Salary
    FROM 
        Staff s
    JOIN 
        Account a ON s.Account_Id = a.Id
    LEFT JOIN 
        (SELECT 
             staff_id, 
             COUNT(*) * 5 AS HoursWorked
         FROM 
             staff_shift
         WHERE 
             Date <= CURDATE()
         GROUP BY 
             staff_id) AS hours_worked ON s.Account_Id = hours_worked.staff_id
    LEFT JOIN 
        (SELECT 
             ss.staff_id, 
             COUNT(o.Id) AS OrderCount
         FROM 
             staff_shift ss
         JOIN 
             `Order` o ON ss.Id = o.staff_shift_id
         GROUP BY 
             ss.staff_id) AS order_count ON s.account_id = order_count.staff_id
    WHERE 
        a.Role_Id = 'Role0002' AND 
        (s.Name LIKE @keyword OR s.account_Id LIKE @keyword);";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@keyword", $"%{keyword}%" }
    };

            // Execute the query to get staff details
            DataTable staffData = DataAccess.DataAccess.ExecuteQuery(staffQuery, parameters);

            // Convert DataTable rows to StaffModel objects
            var result = staffData.AsEnumerable().Select(row => new StaffModel
            {
                Id = row["Id"].ToString(),
                Name = row["Name"].ToString(),
                Account = new AccountModel
                {
                    Status = row["Status"].ToString()
                },
                TotalHourWorked = Convert.ToInt32(row["TotalHourWorked"]),
                TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                AvgOrder = Convert.ToDouble(row["AvgOrder"]),
                Salary = Convert.ToDouble(row["Salary"])
            }).ToList();

            return result;
        }


        public void EditSalary(string staffId, int staffSalary)
        {
            string query = "Update Staff SET salary = @salary Where account_Id = @staffId";
            Dictionary<string, object> parameter = new Dictionary<string, object>
            {
                {"@staffId", staffId },
                {"@salary", staffSalary }
            };
            int count = DataAccess.DataAccess.ExecuteNonQuery(query, parameter);
        }

        public void UpdateShift(string shiftId, string staffId)
        {
            // Define the MySQL UPDATE query
            string query = @"
        UPDATE Staff_Shift 
        SET staff_Id = @StaffId 
        WHERE Id = @ShiftId;
    ";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@StaffId", staffId },
        { "@ShiftId", shiftId }
    };

            // Execute the query
            int count = DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }



        public StaffModel GetStaffbyUserName(string userName)
        {
            // Define the SQL query to get staff details by username
            string query = @"
        SELECT 
            s.Account_Id AS Id,
            s.Name AS Name,
            a.Password AS AccountPassword,
            a.Email AS AccountEmail,
            a.Role_Id AS AccountRoleId,
            s.Salary AS Salary
        FROM 
            Staff s
        JOIN 
            Account a ON s.Account_Id = a.Id
        WHERE 
            a.Username = @Username;
    ";

            // Set up parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Username", userName }
    };

            // Execute the query and get the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if there is a result
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                // Map the result to StaffModel
                var staffModel = new StaffModel
                {
                    Id = row["Id"].ToString(),
                    Name = row["Name"].ToString(),
                    Account = new AccountModel
                    {
                        Password = row["AccountPassword"].ToString(),
                        Email = row["AccountEmail"].ToString(),
                        RoleId = row["AccountRoleId"].ToString()
                    },
                    Salary = Convert.ToDouble(row["Salary"])
                };

                return staffModel;
            }

            return null; // Return null if no staff is found
        }

        public void UpdateStaff(Models.StaffModel staff)
        {
            // Define the SQL query to update the Staff table
            if (!string.IsNullOrEmpty(staff.Name))
            {
                string updateStaffQuery = @"
            UPDATE Staff
            SET Name = @Name
            WHERE Account_Id = @StaffId;
        ";

                var staffParameters = new Dictionary<string, object>
        {
            { "@Name", staff.Name },
            { "@StaffId", staff.Id }
        };

                // Execute the query to update the Staff name
                DataAccess.DataAccess.ExecuteNonQuery(updateStaffQuery, staffParameters);
            }

            // If there is a password update, handle the Account table
            if (!string.IsNullOrEmpty(staff.Account.Password))
            {
                // Assuming that GetStaffByUsernameOrEmail is used to fetch the staff by email
                string getAccountIdQuery = @"
            SELECT Id 
            FROM Account 
            WHERE Email = @Email;
        ";

                var accountParams = new Dictionary<string, object>
        {
            { "@Email", staff.Account.Email }
        };

                DataTable accountIdResult = DataAccess.DataAccess.ExecuteQuery(getAccountIdQuery, accountParams);

                // Check if the account exists
                if (accountIdResult.Rows.Count > 0)
                {
                    string accountId = accountIdResult.Rows[0]["Id"].ToString();

                    // Define the SQL query to update the Account table's password
                    string updateAccountQuery = @"
                UPDATE Account
                SET Password = @Password
                WHERE Id = @AccountId;
            ";

                    var accountUpdateParams = new Dictionary<string, object>
            {
                { "@Password", staff.Account.Password },
                { "@AccountId", accountId }
            };

                    // Execute the query to update the Account password
                    DataAccess.DataAccess.ExecuteNonQuery(updateAccountQuery, accountUpdateParams);
                }
            }
        }



        public List<ShiftSchdeduleModel> GetShiftData(string weekStartDate)
        {
            // Define the SQL query to get shift data
            string query = @"
        SELECT 
    ss.Id AS ShiftId,
    ss.Date AS ShiftDate,
    ss.Shift AS ShiftType,
    ss.Staff_Id AS StaffId,
    s.Name AS StaffName
FROM 
    Staff_Shift ss
LEFT JOIN 
    Staff s ON ss.Staff_Id = s.Account_Id
ORDER BY 
    ss.Date DESC, 
    CASE WHEN ss.Shift = 'morning' THEN 0 ELSE 1 END;
    ";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@WeekStartDate", weekStartDate }
    };

            // Execute the query and retrieve results
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Convert the results to a list of ShiftSchdeduleModel
            var shifts = new List<ShiftSchdeduleModel>();
            foreach (DataRow row in resultTable.Rows)
            {
                shifts.Add(new ShiftSchdeduleModel
                {
                    Id = row["ShiftId"].ToString(),
                    Date = DateOnly.FromDateTime(Convert.ToDateTime(row["ShiftDate"])),
                    Shift = row["ShiftType"].ToString(),
                    StaffId = row["StaffId"].ToString(),
                    StaffName = row["StaffName"].ToString()
                });
            }

            return shifts;
        }


        public void AddShift(DateOnly date, string staffIdMoring, string staffIdAfternoon)
        {
            string sql = "INSERT INTO Staff_Shift(id, date, shift, staff_id) Values(@Id, @Date, @Shift, @StaffId)";
            var dateFormatted = date.ToString("yyyy-MM-dd");

            var parameter = new Dictionary<string, object>
{
    { "@Id", GetNewId() },
    { "@Date", dateFormatted },
    { "@Shift", "Morning" },
    { "@StaffId", staffIdMoring }
};
            DataAccess.DataAccess.ExecuteNonQuery(sql, parameter);

            parameter = new Dictionary<string, object>
            {
                { "@Id", GetNewId() },
                { "Date", dateFormatted },
                { "@Shift", "Afternoon" },
                { "StaffId", staffIdAfternoon }
            };

            DataAccess.DataAccess.ExecuteNonQuery(sql, parameter);

        }

    }
}
