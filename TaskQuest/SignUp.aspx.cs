using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace TaskQuest
{
    public partial class SignUp : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string username = Request.Form["username"];
                string email = Request.Form["email"];
                string password = Request.Form["password"];
                string confirmPassword = Request.Form["confirmPassword"];

                if (password != confirmPassword)
                {
                    Response.Write("<script>alert('Passwords do not match!');</script>");
                    return;
                }

                RegisterUser(username, email, password);
            }
        }

        private void RegisterUser(string username, string email, string password)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["TodoAppDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if Email exists
                    string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                    SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, conn);
                    checkEmailCmd.Parameters.AddWithValue("@Email", email);
                    int emailCount = (int)checkEmailCmd.ExecuteScalar();

                    if (emailCount > 0)
                    {
                        Response.Write("<script>alert('This Email is already registered!');</script>");
                        return;
                    }

                    // Check if Username exists
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, conn);
                    checkUserCmd.Parameters.AddWithValue("@Username", username);
                    int userCount = (int)checkUserCmd.ExecuteScalar();

                    if (userCount > 0)
                    {
                        Response.Write("<script>alert('This Username is already taken!');</script>");
                        return;
                    }

                    // Hash password (simple SHA256)
                    string passwordHash = HashPassword(password);

                    // Insert User
                    string insertQuery = "INSERT INTO Users (Username, Email, PasswordHash, CreatedAt) VALUES (@Username, @Email, @PasswordHash, @CreatedAt)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@Username", username);
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    insertCmd.ExecuteNonQuery();

                    Response.Redirect("Login.aspx");
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message.Replace("'", "\\'") + "');</script>");
                }
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
