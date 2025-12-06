# مستندات ثبت نام و ورود (Login / Signup)

**نویسنده:** Mahsa Dinani
**تاریخ:** 2025-12-06
**پروژه:** TaskQuest

---

## مقدمه
این سند توضیح می‌دهد که چگونه بخش‌های ثبت‌نام و ورود کاربران در پروژه پیاده‌سازی شده‌اند. هدف اصلی، ایجاد یک سیستم ساده و امن برای مدیریت کاربران با استفاده از دیتابیس SQL Server موجود است.

## تغییرات و پیاده‌سازی‌ها

### 1. اتصال به دیتابیس (Connection String)
برای اتصال به دیتابیس `ToDo.mdf` که در مسیر `P:\Dbs` قرار دارد، تغییر زیر در فایل `Web.config` اعمال شد:

```xml
<connectionStrings>
    <add name="TodoAppDB" 
         connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=P:\Dbs\ToDo.mdf;Integrated Security=True;Connect Timeout=30" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```
این تنظیم به برنامه اجازه می‌دهد تا با استفاده از LocalDB به فایل دیتابیس متصل شود.

### 2. صفحه ثبت‌نام (Sign Up)

#### تغییرات ظاهری (Frontend)
در فایل `SignUp.aspx`، دو فیلد جدید اضافه شد تا اطلاعات کامل‌تری از کاربر دریافت شود:
- **نام کاربری (Username):** برای شناسایی کاربر در سیستم.
- **تکرار رمز عبور (Confirm Password):** برای اطمینان از صحت رمز عبور وارد شده.

#### منطق کد (Backend)
در فایل `SignUp.aspx.cs`، متد `RegisterUser` با منطق زیر پیاده‌سازی شده است:

```csharp
private void RegisterUser(string username, string email, string password)
{
    string connectionString = WebConfigurationManager.ConnectionStrings["TodoAppDB"].ConnectionString;

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        try
        {
            conn.Open();

            // 1. Check if Email exists
            string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, conn);
            checkEmailCmd.Parameters.AddWithValue("@Email", email);
            int emailCount = (int)checkEmailCmd.ExecuteScalar();

            if (emailCount > 0)
            {
                Response.Write("<script>alert('This Email is already registered!');</script>");
                return;
            }

            // 2. Check if Username exists
            string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, conn);
            checkUserCmd.Parameters.AddWithValue("@Username", username);
            int userCount = (int)checkUserCmd.ExecuteScalar();

            if (userCount > 0)
            {
                Response.Write("<script>alert('This Username is already taken!');</script>");
                return;
            }

            // 3. Hash password (SHA256)
            string passwordHash = HashPassword(password);

            // 4. Insert User
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
```

**نکات کلیدی:**
1.  **اعتبارسنجی مجزا:** ابتدا ایمیل و سپس نام کاربری بررسی می‌شوند تا خطای دقیق به کاربر نمایش داده شود.
2.  **امنیت:** رمز عبور قبل از ذخیره شدن با استفاده از متد `HashPassword` و الگوریتم SHA256 هش می‌شود.
3.  **جلوگیری از SQL Injection:** استفاده از پارامترها (`Parameters.AddWithValue`) امنیت کوئری‌ها را تضمین می‌کند.

### 3. صفحه ورود (Login)

#### منطق کد (Backend)
در فایل `Login.aspx.cs`، متد `LoginUser` وظیفه احراز هویت را بر عهده دارد:

```csharp
private void LoginUser(string email, string password)
{
    string connectionString = WebConfigurationManager.ConnectionStrings["TodoAppDB"].ConnectionString;

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        try
        {
            conn.Open();

            // 1. Get user by email
            string query = "SELECT Username, PasswordHash FROM Users WHERE Email = @Email";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string dbPasswordHash = reader["PasswordHash"].ToString();
                string username = reader["Username"].ToString();

                // 2. Check password
                if (HashPassword(password) == dbPasswordHash)
                {
                    // Login successful
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
```

**نکات کلیدی:**
1.  **بازیابی اطلاعات:** ابتدا کاربر بر اساس ایمیل جستجو می‌شود.
2.  **بررسی دقیق:** اگر کاربری با آن ایمیل یافت نشد، خطای "User not found" نمایش داده می‌شود.
3.  **تطابق رمز عبور:** اگر کاربر پیدا شد، رمز وارد شده هش شده و با هش دیتابیس مقایسه می‌شود. در صورت عدم تطابق، خطای "Invalid Password" نمایش داده می‌شود.
4.  **مدیریت نشست:** پس از ورود موفق، نام کاربری در `Session` و کوکی احراز هویت (`FormsAuthentication`) ذخیره می‌شود.

## ساختار فرضی دیتابیس
کدها بر اساس این فرض نوشته شده‌اند که جدول `Users` در دیتابیس دارای ستون‌های زیر است:
- `Username` (از نوع متنی)
- `Email` (از نوع متنی)
- `PasswordHash` (از نوع متنی - برای ذخیره رمز عبور)
- `CreatedAt` (از نوع تاریخ)

## نحوه استفاده
برای اجرای پروژه، کافی است برنامه را اجرا کرده و وارد صفحه `SignUp.aspx` شوید. پس از ساخت حساب کاربری، به صفحه `Login.aspx` هدایت می‌شوید تا با اطلاعات خود وارد سیستم شوید.
