using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391_FinalProject.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SWP391_FinalProject.Repository
{
    public class Account
    {
        private readonly DBContext db;

        public Account(DBContext context)
        {
            db = context;
        }
        public string GetNewId()
        {
            string lastId = db.Accounts
                     .OrderByDescending(a => a.Id)
                     .Select(a => a.Id)
                     .FirstOrDefault();
            if (lastId == null)
            {
                return "A0000001";
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

        static string GetMd5Hash(string input)
        {
            // Create an MD5 instance
            using (MD5 md5 = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a StringBuilder to collect the bytes and create a string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                {
                    sb.Append(b.ToString("x2")); // Format each byte as a hexadecimal value
                }

                // Return the hexadecimal string
                return sb.ToString();
            }
        }

        public bool CheckEmail(string email)
        {
            var check = db.Accounts.Where(p => p.Email == email).FirstOrDefault();
            if (check != null)
            {
                return false;
            }
            return true;
        }

        public void AddAccount(Models.AccountModel model)
        {
            string id = GetNewId();
            string md5Password = GetMd5Hash(model.Password);
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
                Point = 0
            };

            Repository.Address addrRepo = new Repository.Address(db);
            id = addrRepo.GetNewId();
            var newAddress = new SWP391_FinalProject.Entities.Address()
            {
                Id = id,
                ProvinceId = model.ProvinceId,
                Address1 = model.Address
            };

            var newUserAddress = new SWP391_FinalProject.Entities.UserAddress()
            {
                UserId = newUser.AccountId,
                AddressId = newAddress.Id,
                IsDefault = ulong.Parse("1"),
            };

            db.Accounts.Add(newAccount);
            db.Users.Add(newUser);
            db.Addresses.Add(newAddress);
            db.UserAddresses.Add(newUserAddress);
            db.SaveChanges();
        }

        public bool Login(string username, string password)
        {
            string mdPassword = GetMd5Hash(password);
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

    }
}
