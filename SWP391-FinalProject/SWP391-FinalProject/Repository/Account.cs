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

            db.Accounts.Add(newAccount);
            db.SaveChanges();
        }
    }
}
