using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace prjLibrarySystem.Models
{
    public class DatabaseHelper
    {
        private static string ConnectionString =
            ConfigurationManager.ConnectionStrings["LibraryDB"]?.ConnectionString ??
            "Data Source=MSI\\SQLEXPRESS;Initial Catalog=dbLibrarySystem;Integrated Security=True";

        // ── Core query methods ───────────────────────────────────────────────

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        public static DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        // ── Connection test (used by reports.aspx.cs) ────────────────────────

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // ── Authentication (used by Login.aspx.cs) ───────────────────────────
        // New schema: tblUsers has UserID as login key, no Username column.
        // Returns a User object if credentials match and account is active.

        public static User AuthenticateUser(string userId, string password)
        {
            string query = @"
                SELECT UserID, Role, FullName, Email, IsActive
                FROM   tblUsers
                WHERE  UserID   = @UserID
                  AND  PasswordHash = @Password
                  AND  IsActive = 1";

            DataTable dt = ExecuteQuery(query, new SqlParameter[]
            {
                new SqlParameter("@UserID",   userId),
                new SqlParameter("@Password", password)
            });

            if (dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];

            return new User
            {
                UserID = row["UserID"].ToString(),
                Role = row["Role"].ToString(),
                FullName = row["FullName"]?.ToString() ?? "",
                Email = row["Email"]?.ToString() ?? "",
                IsActive = true
            };
        }
    }
}