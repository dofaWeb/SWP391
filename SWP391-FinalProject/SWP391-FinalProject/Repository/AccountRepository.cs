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
            // Query to get the last ID from the Accounts table ordered by descending ID
            string query = "SELECT Id FROM Account ORDER BY Id DESC LIMIT 1;";

            // Execute the query and retrieve the result
            DataTable resultTable = DataAccess.DataAccess.ExecuteQuery(query);

            // Check if the result is empty
            if (resultTable.Rows.Count == 0)
            {
                return "A0000001";
            }

            // Get the last ID from the result
            string lastId = resultTable.Rows[0]["Id"].ToString();

            // Split the prefix (e.g., "A") and number (e.g., "0000001")
            string prefix = lastId.Substring(0, 1);
            int number = int.Parse(lastId.Substring(1));

            // Increment the number and format it to 7 digits
            int newNumber = number + 1;
            string newId = $"{prefix}{newNumber:D7}";

            return newId;
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
            AccountRepository AccRepo = new AccountRepository();
            string id = GetNewId();
            string md5Password = MySetting.GetMd5Hash(model.Password);
            var newAccount = new Entities.Account()
            {
                Id = id,
                Username = model.Email,
                Password = md5Password,
                Email = model.Email,
                Phone = "",
                IsActive = ulong.Parse("1"),
                RoleId = "Role0002",
            };

            var newStaff = new Entities.Staff()
            {
                AccountId = id,
                Name = model.Email,
                Salary = 5000000
            };

            db.Accounts.Add(newAccount);
            db.Staff.Add(newStaff);
            db.SaveChanges();
        }

        public void AddAccount(Models.AccountModel model)
        {
            string id = GetNewId();
            string md5Password = MySetting.GetMd5Hash(model.Password);
            var newAccount = new SWP391_FinalProject.Entities.Account()
            {
                Id = id,
                Username = model.Username,
                Password = md5Password,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = ulong.Parse("1"),
                RoleId = "Role0003",
            };

            var newUser = new SWP391_FinalProject.Entities.User()
            {
                AccountId = newAccount.Id,
                Name = model.Name,
                Point = 0,
                Province = model.Province,
                District = model.District,
                Address = model.Address,
            };


            db.Accounts.Add(newAccount);
            db.Users.Add(newUser);

            db.SaveChanges();
        }

        public bool Login(string username, string password)
        {
            string mdPassword = MySetting.GetMd5Hash(password);
            var check = db.Accounts.Where(p => p.Username == username && p.Password == mdPassword).ToList();
            if (check.Any())
            {
                return true;
            }
            return false;
        }

        public string GetRoleId(string username)
        {
            var roleId = db.Accounts.Where(p => p.Username == username).FirstOrDefault();
            return roleId != null ? roleId.RoleId : string.Empty;
        }

        public Models.AccountModel GetUserByUsernameOrEmail(string key)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id
                           join u in db.Users on account.Id equals u.AccountId
                           where account.Username == key || account.Email == key
                           select new Models.AccountModel
                           {
                               Id = account.Id ?? "",
                               Username = account.Username ?? "",
                               Password = account.Password ?? "",
                               Email = account.Email ?? "",
                               Phone = account.Phone ?? "",
                               Name = u.Name ?? "",
                               RoleId = account.RoleId ?? "",
                               Status = (account.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                               RoleName = role.Name ?? "" // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }
        public Models.AccountModel GetStaffByUsernameOrEmail(string key)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id

                           where account.Username == key || account.Email == key
                           select new Models.AccountModel
                           {
                               Id = account.Id,
                               Username = account.Username,
                               Password = account.Password,
                               Email = account.Email,
                               Phone = "",

                               RoleId = account.RoleId,
                               Status = (account.IsActive == ulong.Parse("1")) ? "Active" : "Inactive",
                               RoleName = role.Name  // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }

        public Models.AccountModel GetUserByUsernameAndEmail(string username, string email)
        {
            var userVar = (from account in db.Accounts
                           join role in db.RoleNames on account.RoleId equals role.Id
                           join u in db.Users on account.Id equals u.AccountId
                           where account.Username == username && account.Email == email
                           select new Models.AccountModel
                           {
                               Id = account.Id,
                               Username = account.Username,
                               Password = account.Password,
                               Email = account.Email,
                               Phone = account.Phone,
                               Name = u.Name,
                               RoleId = account.RoleId,
                               Status = account.IsActive.ToString(),
                               RoleName = role.Name // Assuming RoleName is the column you want
                           }).FirstOrDefault();

            return userVar;
        }

        public void ResetPassword(AccountModel account)
        {
            var existingAccount = db.Accounts.FirstOrDefault(a => a.Id == account.Id);
            if (existingAccount != null)
            {
                string mdPassword = MySetting.GetMd5Hash(account.Password);
                existingAccount.Password = mdPassword;
                db.SaveChanges();
            }
        }

        public string GetIdByUsername(string username)
        {
            var id = from account in db.Accounts
                     where account.Username == username
                     select account.Id;
            return id.FirstOrDefault();
        }

        public void UpdateAccount(Models.AccountModel account)
        {

            // Tìm bản ghi theo Id từ cơ sở dữ liệu
            var existingAccount = db.Accounts.FirstOrDefault(a => a.Id == account.Id);

            if (existingAccount != null)
            {
                // Cập nhật các thuộc tính của tài khoản
                if (account.Email != null)
                    existingAccount.Email = account.Email;
                if (account.Phone != null)
                    existingAccount.Phone = account.Phone;
                if (account.Status != null)
                {
                    if (account.Status == "Active")
                    {
                        existingAccount.IsActive = ulong.Parse("1");
                    }
                    else
                    {
                        existingAccount.IsActive = ulong.Parse("0");
                    }
                }
                if (account.RoleId != null)
                    existingAccount.RoleId = account.RoleId;
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }

        }

    }
}
