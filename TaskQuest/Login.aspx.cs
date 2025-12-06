using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace TaskQuest
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string email = Request.Form["email"];
                string password = Request.Form["password"];

                LoginUser(email, password);
            }
        }

        private void LoginUser(string email, string password)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["TodoAppDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get user by email
                    string query = "SELECT Username, PasswordHash FROM Users WHERE Email = @Email";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string dbPasswordHash = reader["PasswordHash"].ToString();
                        string username = reader["Username"].ToString();

                        // Check password
                        if (HashPassword(password) == dbPasswordHash)
                        {
                            // Login successful
                            // Set cookie or session
                            Session["User"] = username;
                            FormsAuthentication.SetAuthCookie(username, false);
                            Response.Redirect("Default.aspx");
                        }
                        else
                        {
                            Response.Write("<script>alert('Invalid Password!');</script>");
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('User not found!');</script>");
                    }
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
