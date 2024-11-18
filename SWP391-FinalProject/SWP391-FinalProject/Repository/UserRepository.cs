using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Data;
using System.Security.Principal;

namespace SWP391_FinalProject.Repository
{
    public class UserRepository
    {
        
        public UserRepository()
        {
            
        }

        public bool CheckBan(string UserId)
        {
            string query = "SELECT * FROM Account Where is_active = 1 And id = @UserId";
            var parameter = new Dictionary<string, object>
            {
                { "@UserId", UserId }
            };
            var result = DataAccess.DataAccess.ExecuteQuery(query, parameter);
            if (result.Rows.Count > 0)
                return true;
            return false;
        }

        public UserModel GetUserProfileByUsername(string username)
        {
            // Define the SQL query to fetch user profile details
            string query = @"
        SELECT 
            a.Id AS AccountId,
            a.Username,
            a.Password,
            a.Email,
            a.Phone,
            a.is_active,
            a.role_id,
            r.Name AS RoleName,
            u.Name AS Name,
            u.Point,
            u.Province,
            u.District,
            u.Address
        FROM Account a
        JOIN User u ON a.Id = u.account_id
        LEFT JOIN Role_Name r ON a.role_id = r.Id
        WHERE a.Username = @Username";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Username", username }
    };

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count == 0)
                throw new ArgumentException($"User with username '{username}' not found.");

            // Map DataTable result to UserModel
            DataRow row = resultTable.Rows[0];
            var userProfile = new UserModel
            {
                Account = new AccountModel
                {
                    Id = row["AccountId"].ToString(),
                    Username = row["Username"].ToString(),
                    Password = row["Password"].ToString(),
                    Name = row["Name"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Status = (row["is_active"].ToString() == "1") ? "Active" : "Inactive",
                    RoleId = row["role_id"].ToString(),
                    RoleName = row["RoleName"].ToString()
                },
                Name = row["Name"].ToString(),
                Point = Convert.ToInt32(row["Point"]),
                Province = row["Province"].ToString(),
                District = row["District"].ToString(),
                Address = row["Address"].ToString()
            };

            return userProfile;
        }


        public UserModel GetUserProfileByUserId(string userId)
        {
            // Define the SQL query to fetch user profile details
            string query = @"
        SELECT 
            a.Id AS AccountId,
            a.Username,
            a.Password,
            a.Email,
            a.Phone,
            a.is_active,
            a.role_id,
            r.Name AS RoleName,
            u.Name AS UserName,
            u.Point,
            u.Province,
            u.District,
            u.Address
        FROM Account a
        JOIN `User` u ON a.Id = u.account_id
        LEFT JOIN Role_Name r ON a.role_id = r.Id
        WHERE a.Id = @UserId";

            // Define parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@UserId", userId }
    };

            // Execute the query and get results as a DataTable
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count == 0)
                return null; // Return null if no results found

            // Map DataTable result to UserModel
            DataRow row = resultTable.Rows[0];
            var userProfile = new UserModel
            {
                Account = new AccountModel
                {
                    Id = row["AccountId"].ToString(),
                    Username = row["Username"].ToString(),
                    Password = row["Password"].ToString(),
                    Name = row["Username"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Status = (row["is_active"].ToString() == "1") ? "Active" : "Inactive",
                    RoleId = row["role_id"].ToString(),
                    RoleName = row["RoleName"].ToString()
                },
                Name = row["UserName"].ToString(),
                Point = Convert.ToInt32(row["Point"]),
                Province = row["Province"].ToString(),
                District = row["District"].ToString(),
                Address = row["Address"].ToString()
            };

            return userProfile;
        }

        public string GetUserIdByUserName(string UserName)
        {
            // Define the SQL query to get the user ID by username
            string query = "SELECT Id FROM Account WHERE Username = @Username LIMIT 1";

            // Define the parameter for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Username", UserName }
    };

            // Execute the query
            DataTable result = DataAccess.DataAccess.ExecuteQuery(query, parameters);

            // Check if any rows are returned
            if (result.Rows.Count == 0)
            {
                return null; // No matching record found
            }

            // Get the user ID from the first row
            return result.Rows[0]["Id"].ToString();
        }

        public void UpdateUser(UserModel User)
        {
            // Step 1: Get the Account ID, Status, and RoleId by Username or Email
            AccountRepository accRepo = new AccountRepository();
            AccountModel Acc = accRepo.GetUserByUsernameOrEmail(User.Account.Username);
            User.Account.Id = Acc.Id;
            User.Account.Status = Acc.Status;
            User.Account.RoleId = Acc.RoleId;

            // Update account details
            accRepo.UpdateAccount(User.Account);

            // Step 2: Update user details in the Users table
            string query = @"
        UPDATE `User`
        SET Name = @Name,
            Province = @Province,
            District = @District,
            Address = @Address,
            Point = CASE WHEN @Point = 0 THEN Point ELSE @Point END
        WHERE account_id = @AccountId";

            // Define the parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@Name", User.Name },
        { "@Province", User.Province },
        { "@District", User.District },
        { "@Address", User.Address },
        { "@Point", User.Point },
        { "@AccountId", User.Account.Id }
    };

            // Execute the query to update the Users table
            DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }


        public void UpdateUserPoint(string username, int OrderStateId, decimal? UsePoint, decimal? EarnPoint)
        {
            // Step 1: Retrieve the current points of the user by username
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(username);

            // Step 2: Calculate the updated points based on OrderStateId
            int updatedPoints = user.Point;

            if (OrderStateId == 1)
            {
                updatedPoints -= UsePoint.HasValue ? (int)UsePoint.Value : 0;
            }
            else if (OrderStateId == 2)
            {
                updatedPoints += EarnPoint.HasValue ? (int)EarnPoint.Value : 0;
            }
            else if (OrderStateId == 3 || OrderStateId == 4)
            {
                updatedPoints += UsePoint.HasValue ? (int)UsePoint.Value : 0;
            }

            // Step 3: SQL query to update points in Users table
            string query = "UPDATE `User` SET Point = @UpdatedPoints WHERE account_id = @AccountId";

            // Define the parameters for the query
            var parameters = new Dictionary<string, object>
    {
        { "@UpdatedPoints", updatedPoints },
        { "@AccountId", user.Account.Id }
    };

            // Execute the update query
            DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }


        public void BanUserById(string id)
        {
            // Define the query to toggle the IsActive status
            string query = @"
        UPDATE `Account`
        SET is_active = CASE 
            WHEN is_active = 1 THEN 0 
            ELSE 1 
        END
        WHERE Id = @UserId";

            // Define the parameter for the query
            var parameters = new Dictionary<string, object>
    {
        { "@UserId", id }
    };

            // Execute the update query
            DataAccess.DataAccess.ExecuteNonQuery(query, parameters);
        }

    }
}
