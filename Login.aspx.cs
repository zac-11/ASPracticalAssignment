using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace PracticalAssignment
{
    public partial class Login : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        byte[] IV;
        byte[] Key;

        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Login_Click(object sender, EventArgs e)
        {
            // Captcha validation
            if (ValidateCaptcha())
            {
                string email = HttpUtility.HtmlEncode(tb_email.Text.ToString().Trim());
                string password = HttpUtility.HtmlEncode(tb_password.Text.ToString().Trim());

                SHA512Managed hashing = new SHA512Managed();
                string dbHash = getDBHash(email);
                string dbSalt = getDBSalt(email);

                int failedAttempts = getLockoutCount(email);
                DateTime latestLockout = getDateTimeLockout(email);

                lbl_errorMsg.Visible = true;
                try
                {
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string passwordWithSalt = password + dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);

                        if (userHash.Equals(dbHash))
                        {
                            if (DateTime.Now > latestLockout)
                            {
                                Session["LoggedIn"] = HttpUtility.HtmlEncode(tb_email.Text.Trim());
                                string guid = Guid.NewGuid().ToString();
                                Session["AuthToken"] = guid;
                                Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                                Response.Redirect("HomePage.aspx", false);
                                setLockoutCount(email, 0);
                                setLatestLockout(email, DateTime.Now);


                            }
                            else
                            {
                                if (failedAttempts >= 3)
                                {
                                    lbl_errorMsg.Text = "Your account has been locked out due to 3 failed attempts. " +
                                        "Account will unlock in " + Convert.ToInt32(latestLockout.Subtract(DateTime.Now).TotalSeconds) + " seconds.";
                                    lbl_errorMsg.ForeColor = System.Drawing.Color.Red;
                                }
                                else if (failedAttempts < 3)
                                {
                                    Session["LoggedIn"] = HttpUtility.HtmlEncode(tb_email.Text.Trim());
                                    string guid = Guid.NewGuid().ToString();
                                    Session["AuthToken"] = guid;
                                    Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                                    Response.Redirect("HomePage.aspx", false);
                                    setLockoutCount(email, 0);
                                    setLatestLockout(email, DateTime.Now);
                                }
                            }
                            
                        }
                        else
                        {
                            failedAttempts = getLockoutCount(email);
                            if (DateTime.Now > latestLockout && failedAttempts == 3)
                            {
                                lbl_errorMsg.Text = "Your account has been unlocked after 1 minute of account lockout.";
                                lbl_errorMsg.ForeColor = System.Drawing.Color.Green;
                                setLockoutCount(email, 0);
                            }
                            else
                            {
                                failedAttempts++;
                                if (failedAttempts >= 3)
                                {
                                    lbl_errorMsg.Text = "Your account has been locked out due to 3 failed attempts. " +
                                        "Account will unlock in " + Convert.ToInt32(latestLockout.Subtract(DateTime.Now).TotalSeconds) + " seconds.";
                                    lbl_errorMsg.ForeColor = System.Drawing.Color.Red;
                                    setLockoutCount(email, 3);
                                }
                                else if (failedAttempts < 3)
                                {
                                    lbl_errorMsg.Text = "Email or password is not valid. Please try again. " +
                                        "You have " + (3 - failedAttempts) + " attempts remaining before account lockout.";
                                    lbl_errorMsg.ForeColor = System.Drawing.Color.Red;
                                    setLockoutCount(email, failedAttempts);
                                    setLatestLockout(email, DateTime.Now.AddMinutes(1));
                                }
                            }
                        }
                    }
                    //lbl_errorMsg.Text = "Email or password is not valid. Please try again.";
                    //lbl_errorMsg.ForeColor = System.Drawing.Color.Red;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                finally { }
            }
        }

        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "Select PasswordHash FROM Account WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);

            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            { h = reader["PasswordHash"].ToString(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return h;

        }

        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "Select PasswordSalt FROM Account WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);

            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordSalt"] != null)
                        {
                            if (reader["PasswordSalt"] != DBNull.Value)
                            { s = reader["PasswordSalt"].ToString(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return s;
        }

        protected int getLockoutCount(string email)
        {
            int count = 0;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "Select LockoutCount FROM Account WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);

            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockoutCount"] != null)
                        {
                            if (reader["LockoutCount"] != DBNull.Value)
                            { count = Convert.ToInt32(reader["LockoutCount"].ToString()); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return count;
        }

        protected DateTime getDateTimeLockout(string email)
        {
            DateTime date = new DateTime();
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "SELECT DateTimeLockout FROM Account WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);

            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["DateTimeLockout"] != null)
                        {
                            if (reader["DateTimeLockout"] != DBNull.Value)
                            { date = Convert.ToDateTime(reader["DateTimeLockout"].ToString()); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return date;
        }

        protected int setLockoutCount(string email, int count)
        {
            int i = 0;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "UPDATE Account SET LockoutCount=@LockoutCount WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@LockoutCount", count);
            cmd.Parameters.AddWithValue("@Email", email);
            
            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockoutCount"] != null)
                        {
                            if (reader["LockoutCount"] != DBNull.Value)
                            { i = Convert.ToInt32(reader["LockoutCount"].ToString()); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return i;
        }

        protected int setLatestLockout(string email, DateTime dateTime)
        {
            int i = 0;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = "UPDATE Account SET DateTimeLockout=@DateTimeLockout WHERE Email=@Email";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@DateTimeLockout", dateTime);

            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockoutCount"] != null)
                        {
                            if (reader["LockoutCount"] != DBNull.Value)
                            { i = Convert.ToInt32(reader["LockoutCount"].ToString()); }
                        }
                    }
                }
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { con.Close(); }
            return i;
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { }
            return cipherText;
        }

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret= &response=" + captchaResponse);
            // removed secret key
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();
                        lbl_captchaMsg.Text = jsonResponse.ToString();
                        lbl_captchaMsg.ForeColor = System.Drawing.Color.Blue;
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex) { throw ex; }
        }
    }
}
