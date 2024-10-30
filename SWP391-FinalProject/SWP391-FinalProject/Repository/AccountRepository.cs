using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Helpers;
using SWP391_FinalProject.Models;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SWP391_FinalProject.Repository
{
    public class AccountRepository
    {
        private readonly DBContext db;

        public AccountRepository()
        {
            db = new DBContext();
        }
        public string GetNewId()
        {
            // Define the prefix for account IDs
            string prefix = "A"; // Adjust this if your prefix changes

            // Dynamic query to get the last ID with the specified prefix
            string query = $"SELECT Id FROM Account WHERE Id LIKE '{prefix}%' ORDER BY Id DESC LIMIT 1;";

            // Execute the query and get the results
            DataTable existingAccounts = DataAccess.DataAccess.ExecuteQuery(query);

            int nextIdNumber = 1; // Default to 1 if no existing ID

            if (existingAccounts != null && existingAccounts.Rows.Count > 0)
            {
                // Retrieve the last ID from the result set
                string lastId = existingAccounts.Rows[0]["Id"].ToString();

                // Extract the numeric part and increment
                nextIdNumber = int.Parse(lastId.Substring(prefix.Length)) + 1;
            }

            // Generate new ID with the specified format
            return $"{prefix}{nextIdNumber:D7}"; // Format as "A0000001", etc.
        }



        public List<Models.AccountModel> GetAllAccount()
        {
            // SQL query to select account details and join with the Role table
            string query = @"
        SELECT 
            a.Id, 
            a.Username, 
            a.Email, 
            a.Phone, 
            CASE WHEN a.is_active = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
            r.Name AS RoleName
        FROM 
            Account a
        LEFT JOIN 
            Role_Name r ON a.role_id = r.Id;
    ";

            // Execute the query and store the result in a DataTable
            DataTable accountTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Convert the result into a list of AccountModel objects
            var accounts = new List<Models.AccountModel>();
            foreach (DataRow row in accountTable.Rows)
            {
                accounts.Add(new Models.AccountModel
                {
                    Id = row["Id"].ToString(),
                    Username = row["Username"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Status = row["Status"].ToString(),
                    RoleName = row["RoleName"].ToString()
                });
            }

            return accounts;
        }


        public Models.AccountModel GetAccountById(string id)
        {
            // SQL query to select account and user details based on the account ID
            string query = $@"
        SELECT 
            a.Id, 
            a.Username, 
            u.Name AS Name, 
            a.Email, 
            a.Phone, 
            u.Point
        FROM 
            Account a
        LEFT JOIN 
            User u ON a.Id = u.account_id
        WHERE 
            a.Id = '{id}';
    ";

            // Execute the query and store the result in a DataTable
            DataTable accountTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if any rows were returned
            if (accountTable.Rows.Count == 0)
            {
                return null; // Return null if no account with the given ID was found
            }

            // Convert the result into an AccountModel object
            DataRow row = accountTable.Rows[0];
            var account = new Models.AccountModel
            {
                Id = row["Id"].ToString(),
                Username = row["Username"].ToString(),
                Name = row["Name"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"].ToString(),
                Point = Convert.ToInt32(row["Point"])
            };

            return account;
        }




        public bool CheckEmail(string email)
        {
            // SQL query to check if any account has the specified email
            string query = $@"
        SELECT COUNT(1) 
        FROM Account 
        WHERE email = '{email}';
    ";

            // Execute the query and store the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if any records were found
            int count = Convert.ToInt32(resultTable.Rows[0][0]);

            // If count is greater than 0, email exists, so return false; otherwise, return true
            return count == 0;
        }


        public bool CheckUsername(string username)
        {
            // SQL query to check if any account has the specified username
            string query = $@"
        SELECT COUNT(1) 
        FROM Account 
        WHERE username = '{username}';
    ";

            // Execute the query and store the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if any records were found
            int count = Convert.ToInt32(resultTable.Rows[0][0]);

            // If count is greater than 0, username exists, so return false; otherwise, return true
            return count == 0;
        }


        public void AddStaffAccount(AccountModel model)
        {
            string id = GetNewId(); // Generate a new account ID
            string md5Password = MySetting.GetMd5Hash(model.Password); // Hash the password

            // Prepare the SQL query for inserting a new account
            string insertAccountQuery = @"
        INSERT INTO Account (Id, Username, Password, Email, Phone, is_active, role_id) 
        VALUES (@Id, @Username, @Password, @Email, @Phone, @is_active, @role_id);
    ";

            // Execute the query to insert the new account
            var accountResult = DataAccess.DataAccess.ExecuteNonQuery(insertAccountQuery, new Dictionary<string, object>
    {
        { "@Id", id },
        { "@Username", model.Email },
        { "@Password", md5Password },
        { "@Email", model.Email },
        { "@Phone", "" }, // Assuming phone is not provided
        { "@is_active", ulong.Parse("1") },
        { "@role_id", "Role0002" }
    });

            // Prepare the SQL query for inserting a new staff record
            string insertStaffQuery = @"
        INSERT INTO Staff (account_id, Name, Salary) 
        VALUES (@account_id, @Name, @Salary);
    ";

            // Execute the query to insert the new staff
            var staffResult = DataAccess.DataAccess.ExecuteNonQuery(insertStaffQuery, new Dictionary<string, object>
    {
        { "@account_id", id },
        { "@Name", model.Email },
        { "@Salary", 5000000 }
    });

            // Optionally, you could check accountResult and staffResult for success/failure
        }

        public void AddAccount(Models.AccountModel model)
        {
            string id = GetNewId(); // Generate a new account ID
            string md5Password = MySetting.GetMd5Hash(model.Password); // Hash the password

            // Prepare the SQL query for inserting a new account
            string insertAccountQuery = @"
        INSERT INTO Account (Id, Username, Password, Email, Phone, is_active, role_id) 
        VALUES (@Id, @Username, @Password, @Email, @Phone, @is_active, @role_id);
    ";

            // Execute the query to insert the new account
            var accountResult = DataAccess.DataAccess.ExecuteNonQuery(insertAccountQuery, new Dictionary<string, object>
    {
        { "@Id", id },
        { "@Username", model.Username },
        { "@Password", md5Password },
        { "@Email", model.Email },
        { "@Phone", model.Phone },
        { "@is_active", ulong.Parse("1") },
        { "@role_id", "Role0003" }
    });

            // Prepare the SQL query for inserting a new user
            string insertUserQuery = @"
       INSERT INTO User (account_id, Name, Point, Province, District, Address) 
        VALUES (@account_id, @Name, @Point, @Province, @District, @Address);
    ";

            // Execute the query to insert the new user
            var userResult = DataAccess.DataAccess.ExecuteNonQuery(insertUserQuery, new Dictionary<string, object>
    {
        { "@account_id", id },
        { "@Name", model.Name },
        { "@Point", 0 }, // Initial point value
        { "@Province", model.Province },
        { "@District", model.District },
        { "@Address", model.Address }
    });

            // Optionally, you could check accountResult and userResult for success/failure
        }

        public AccountModel GetAccountByUsernameAndPassword(string username, string password)
        {
            string mdPassword = MySetting.GetMd5Hash(password);
            string query = "Select * From Account " +
                            "Where username=@Username And password=@Password";
            var resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object> {
                {"@Username", username},
                {"@Password", mdPassword}
            });
            AccountModel account = null;
            if (resultTable != null)
            {
                var row = resultTable.Rows[0];
                account = new AccountModel()
                {
                    Id = row["id"].ToString(),
                    Username = row["username"].ToString(),
                    Password = row["password"].ToString(), // Consider if you really want to expose the password
                    Email = row["email"].ToString(),
                    Phone = row["phone"].ToString(),
                    RoleId = row["role_id"].ToString(),
                    Status = ((int.Parse(row["is_active"].ToString()))==1?"Active": "Inactive"),
                };
            }
            return account;
        }

        public bool Login(string username, string password)
        {
            string mdPassword = MySetting.GetMd5Hash(password); // Hash the provided password

            // Prepare the SQL query to check for a matching account
            string query = @"
        SELECT COUNT(1) 
        FROM Account 
        WHERE username = @Username AND password = @Password;
    ";

            // Execute the query and store the result
            var resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
             {
        { "@Username", username },
        { "@Password", mdPassword }
              });

            // Check if any records were found
            int count = Convert.ToInt32(resultTable.Rows[0][0]);

            // If count is greater than 0, the login is successful; otherwise, it fails
            return count > 0;
        }

        public string GetRoleId(string username)
        {
            // Prepare the SQL query to select the RoleId for the specified username
            string query = @"
        SELECT role_id 
        FROM Account 
        WHERE Username = @Username;
    ";

            // Execute the query and store the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
    {
        { "@Username", username }
    });

            // Check if any records were returned
            if (resultTable.Rows.Count > 0)
            {
                // Return the RoleId from the first row
                return resultTable.Rows[0]["role_id"].ToString();
            }

            // Return an empty string if no records were found
            return string.Empty;
        }


        public Models.AccountModel GetUserByUsernameOrEmail(string key)
        {
            // Prepare the SQL query to select user details based on username or email
            string query = @"
        SELECT 
            a.Id, 
            a.Username, 
            a.Password, 
            a.Email, 
            a.Phone, 
            u.Name, 
            a.role_id, 
            CASE WHEN a.is_active = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
            r.Name AS RoleName
        FROM 
            Account a
        JOIN 
            Role_Name r ON a.role_id = r.Id
        JOIN 
            User u ON a.Id = u.account_id
        WHERE 
            a.Username = @Key OR a.Email = @Key;
    ";

            // Execute the query and store the result in a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
    {
        { "@Key", key }
    });

            // Check if any records were returned
            if (resultTable.Rows.Count > 0)
            {
                // Return the first result mapped to an AccountModel
                var row = resultTable.Rows[0];
                return new Models.AccountModel
                {
                    Id = row["Id"].ToString(),
                    Username = row["Username"].ToString(),
                    Password = row["Password"].ToString(), // Consider if you really want to expose the password
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Name = row["Name"].ToString(),
                    RoleId = row["role_id"].ToString(),
                    Status = row["Status"].ToString(),
                    RoleName = row["RoleName"].ToString()
                };
            }

            // Return null if no records were found
            return null;
        }

        public Models.AccountModel GetStaffByUsernameOrEmail(string key)
        {
            // Prepare the SQL query to select staff details based on username or email
            string query = @"
        SELECT 
            a.Id, 
            a.Username, 
            a.Password, 
            a.Email, 
            a.Phone, 
            a.role_id, 
            CASE WHEN a.is_active = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
            r.Name AS RoleName
        FROM 
            Account a
        JOIN 
            Role_Name r ON a.role_id = r.Id
        WHERE 
            a.Username = @Key OR a.Email = @Key;
    ";

            // Execute the query and store the result in a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
    {
        { "@Key", key }
    });

            // Check if any records were returned
            if (resultTable.Rows.Count > 0)
            {
                // Return the first result mapped to an AccountModel
                var row = resultTable.Rows[0];
                return new Models.AccountModel
                {
                    Id = row["Id"].ToString(),
                    Username = row["Username"].ToString(),
                    Password = row["Password"].ToString(), // Consider if you really want to expose the password
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    RoleId = row["role_id"].ToString(),
                    Status = row["Status"].ToString(),
                    RoleName = row["RoleName"].ToString()
                };
            }

            // Return null if no records were found
            return null;
        }


        public Models.AccountModel GetUserByUsernameAndEmail(string username, string email)
        {
            // Prepare the SQL query to select user details based on username and email
            string query = @"
        SELECT 
            a.Id, 
            a.Username, 
            a.Password, 
            a.Email, 
            a.Phone, 
            u.Name, 
            a.role_id, 
            CASE WHEN a.is_active = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
            r.Name AS RoleName
        FROM 
            Account a
        JOIN 
            Role_Name r ON a.role_id = r.Id
        JOIN 
            User u ON a.Id = u.account_id
        WHERE 
            a.Username = @Username AND a.Email = @Email;
    ";

            // Execute the query and store the result in a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
    {
        { "@Username", username },
        { "@Email", email }
    });

            // Check if any records were returned
            if (resultTable.Rows.Count > 0)
            {
                // Return the first result mapped to an AccountModel
                var row = resultTable.Rows[0];
                return new Models.AccountModel
                {
                    Id = row["Id"].ToString(),
                    Username = row["Username"].ToString(),
                    Password = row["Password"].ToString(), // Consider if you really want to expose the password
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Name = row["Name"].ToString(),
                    RoleId = row["role_id"].ToString(),
                    Status = row["Status"].ToString(),
                    RoleName = row["RoleName"].ToString()
                };
            }

            // Return null if no records were found
            return null;
        }

        public void ResetPassword(AccountModel account)
        {
            // Prepare the SQL query to update the password for the specified account ID
            string query = @"
        UPDATE Account 
        SET Password = @Password 
        WHERE Id = @account_id;
    ";

            // Hash the new password
            string mdPassword = MySetting.GetMd5Hash(account.Password);

            // Execute the query with parameters to prevent SQL injection
            var rowsAffected = DataAccess.DataAccess.ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@Password", mdPassword },
        { "@account_id", account.Id }
    });

            // Optionally, you can check rowsAffected to ensure the update was successful
            if (rowsAffected == 0)
            {
                // Handle the case where the account was not found or not updated
                // This could be logging an error, throwing an exception, etc.
            }
        }


        public string GetIdByUsername(string username)
        {
            // Prepare the SQL query to select the account ID based on the username
            string query = @"
        SELECT Id 
        FROM Account 
        WHERE Username = @Username;
    ";

            // Execute the query and store the result in a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, new Dictionary<string, object>
    {
        { "@Username", username }
    });

            // Check if any records were returned
            if (resultTable.Rows.Count > 0)
            {
                // Return the first result (account ID)
                return resultTable.Rows[0]["Id"].ToString();
            }

            // Return null if no records were found
            return null;
        }


        public void UpdateAccount(Models.AccountModel account)
        {
            // Prepare the SQL query to update the account properties based on the provided account ID
            string query = @"
        UPDATE Account 
        SET 
            Email = @Email,
            Phone = @Phone,
            is_active = @is_active,
            role_id = @role_id
        WHERE 
            Id = @account_id;
    ";

            // Determine the active status value
            int is_active = account.Status == "Active" ? 1 : 0;

            // Execute the query with parameters to update the account details
            var rowsAffected = DataAccess.DataAccess.ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@Email", account.Email ?? (object)DBNull.Value }, // Handle nulls
        { "@Phone", account.Phone ?? (object)DBNull.Value }, // Handle nulls
        { "@is_active", is_active },
        { "@role_id", account.RoleId ?? (object)DBNull.Value }, // Handle nulls
        { "@account_id", account.Id }
    });

            // Optionally, you can check rowsAffected to ensure the update was successful
            if (rowsAffected == 0)
            {
                // Handle the case where the account was not found or not updated
                // This could be logging an error, throwing an exception, etc.
            }
        }


    }
}
