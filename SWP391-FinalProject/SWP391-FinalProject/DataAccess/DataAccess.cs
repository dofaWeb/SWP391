using System.Data;
using MySql.Data.MySqlClient;

namespace SWP391_FinalProject.DataAccess
{
    public class DataAccess
    {
        private static readonly string _connectionString = "server=mysql-35c69a44-swp391-group3.i.aivencloud.com;Port=25832;database=SWP391;user=avnadmin;password=AVNS_mzCmZ_1hz1gM4yr03o8;sslmode=Required;";

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

        // Method to execute an UPDATE, INSERT, or DELETE query and return the number of affected rows
        public static int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
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

                    // Execute the query and return the number of affected rows
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
