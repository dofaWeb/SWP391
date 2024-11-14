using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using SWP391_FinalProject.Entities;
using SWP391_FinalProject.Models;
using System.Security.Cryptography.X509Certificates;

namespace SWP391_FinalProject.DataAccess
{
    public class DataAccess
    {
        //private static readonly string _connectionString = "server=mysql-35c69a44-swp391-group3.i.aivencloud.com;Port=25832;database=SWP391;user=avnadmin;password=AVNS_mzCmZ_1hz1gM4yr03o8;sslmode=Required;";

        private static readonly string _connectionString = "server=127.0.0.1;port=3306;database=swp391_local;user=root;sslmode=None;";
        public DataAccess()
        {

        }

        // Method to execute a SELECT query and return results as a DataTable
        public static DataTable ExecuteQuery(string query)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    // Add parameters if provided
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        // Method to execute an UPDATE, INSERT, or DELETE query and return the number of affected rows
        public static int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    // Thêm tham số nếu có
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            // Kiểm tra nếu giá trị của tham số là null
                            if (param.Value == null)
                            {
                                throw new ArgumentException($"Parameter '{param.Key}' cannot be null.");
                            }
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    // Thực hiện truy vấn và trả về số hàng bị ảnh hưởng
                    return cmd.ExecuteNonQuery();
                }
            }
        }

       

    }
}
