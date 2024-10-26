using SWP391_FinalProject.Models;
using System.Security.Cryptography;
using System.Text;

namespace SWP391_FinalProject.Helpers
{
    public class MySetting
    {
        public static string CLAIM_CUSTOMERID = "CustomerID";
        public static string CLAIM_STAFFID = "StaffID";


        public static int Otp;

        public static AccountModel Account;

        public static string GetMd5Hash(string input)
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
    }
}
