using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }
                else
                {
                    lbl_message.Text = "Change Password?";
                    lbl_message.ForeColor = Color.Green;
                }
            }
            else
            {
                Response.Redirect("CustomError/HTTP403.html", false);
            }
        }

        protected void btn_ChangePassword_Click(object sender, EventArgs e)
        {
            if (passwordValidated() is true)
            {
                string email = Session["LoggedIn"].ToString();
                string SecondPasswordHash = getDBHash("current", email);
                string SecondPasswordSalt = getDBSalt("current", email);
                string ThirdPasswordHash = getDBHash("second", email);
                string ThirdPasswordSalt = getDBSalt("second", email);
                System.Diagnostics.Debug.WriteLine(ThirdPasswordHash);
                

                string newPassword = HttpUtility.HtmlEncode(tb_newPassword.Text.ToString().Trim());

                //Generate random "salt" 
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] saltByte = new byte[8];

                //Fills array of bytes with a cryptographically strong sequence of random values.
                rng.GetBytes(saltByte);
                salt = Convert.ToBase64String(saltByte);

                SHA512Managed hashing = new SHA512Managed();

                string pwdWithSalt = newPassword + salt;
                byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                finalHash = Convert.ToBase64String(hashWithSalt);

                RijndaelManaged cipher = new RijndaelManaged();
                cipher.GenerateKey();
                Key = cipher.Key;
                IV = cipher.IV;

                try
                {
                    using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE Account SET PasswordHash=@PasswordHash, PasswordSalt=@PasswordSalt, SecondPasswordHash=@SecondPasswordHash, SecondPasswordSalt=@SecondPasswordSalt, ThirdPasswordHash=@ThirdPasswordHash, ThirdPasswordSalt=@ThirdPasswordSalt, DateTimePasswordChanged=@DateTimePasswordChanged WHERE Email=@Email"))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                                cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                                cmd.Parameters.AddWithValue("@SecondPasswordHash", SecondPasswordHash);
                                cmd.Parameters.AddWithValue("@SecondPasswordSalt", SecondPasswordSalt);
                                if (ThirdPasswordHash == null)
                                {
                                    cmd.Parameters.AddWithValue("@ThirdPasswordHash", DBNull.Value);
                                } else
                                {
                                    cmd.Parameters.AddWithValue("@ThirdPasswordHash", ThirdPasswordHash);
                                }
                                if (ThirdPasswordSalt == null)
                                {
                                    cmd.Parameters.AddWithValue("@ThirdPasswordSalt", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@ThirdPasswordSalt", ThirdPasswordSalt);
                                }
                                
                                cmd.Parameters.AddWithValue("@DateTimePasswordChanged", DateTime.Now);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.Connection = con;
                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                { throw new Exception(ex.ToString()); }
                                finally
                                { con.Close(); }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                // After changing password, perform user logout
                Session.Clear();
                Session.Abandon();
                Session.RemoveAll();
                Response.Redirect("Login.aspx", false);
                if (Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                    Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
                }
                if (Request.Cookies["AuthToken"] != null)
                {
                    Response.Cookies["AuthToken"].Value = string.Empty;
                    Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
                }
            }
        }

        protected void onPasswordTextChanged(object sender, EventArgs e)
        {
            int scores = checkPassword(HttpUtility.HtmlEncode(tb_newPassword.Text));
            string status = "";
            switch (scores)
            {
                case 1:
                    status = "Very Weak";
                    break;
                case 2:
                    status = "Weak";
                    break;
                case 3:
                    status = "Medium";
                    break;
                case 4:
                    status = "Strong";
                    break;
                case 5:
                    status = "Excellent";
                    break;
                default:
                    break;
            }
            lbl_passwordChecker2.Text = "Status: " + status;
            if (scores < 4)
            {
                lbl_passwordChecker2.ForeColor = Color.Red;
                return;
            }
            lbl_passwordChecker2.ForeColor = Color.Green;
        }

        private int checkPassword(string password)
        {
            int score = 0;
            if (password.Length < 12) { return 1; }
            else { score = 1; }
            if (Regex.IsMatch(password, "[a-z]")) { score++; }
            if (Regex.IsMatch(password, "[A-Z]")) { score++; }
            if (Regex.IsMatch(password, "[0-9]")) { score++; }
            if (Regex.IsMatch(password, "[^A-Za-z0-9]")) { score++; }
            return score;
        }
        protected bool passwordValidated()
        {
            string email = Session["LoggedIn"].ToString();
            bool isValid = true;
            string currentPassword = HttpUtility.HtmlEncode(tb_currentPassword.Text.ToString().Trim());
            string newPassword = HttpUtility.HtmlEncode(tb_newPassword.Text.ToString().Trim());
            //passwordInDatabase(email, currentPassword);// should return true, meaning password is in db
            //secondPasswordInDatabase(email, newPassword); // should return false, meaning password not in db
            //thirdPasswordInDatabase(email, newPassword); // should return false, meaning password not in db
            if (passwordInDatabase(email, currentPassword) is false)
            {
                lbl_currentPassword.Text = "Current password is incorrect";
                lbl_currentPassword.ForeColor = Color.Red;
                isValid = false;

                if (secondPasswordInDatabase(email, newPassword) || thirdPasswordInDatabase(email, newPassword))
                {
                    lbl_passwordChecker2.Text = "Your new password cannot be the same as any of your recent passwords.";
                    lbl_passwordChecker2.ForeColor = Color.Red;
                    isValid = false;
                }
            }
            return isValid;
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
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception ex)
            { throw new Exception(ex.ToString()); }
            finally { }
            return cipherText;
        }

        protected bool passwordInDatabase(string email, string currentPassword)
        {
            var passwordInDatabase = false;
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash("current", email);
            string dbSalt = getDBSalt("current", email);
            if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
            {
                string passwordWithSalt = currentPassword + dbSalt;
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                string userHash = Convert.ToBase64String(hashWithSalt);
                if (userHash.Equals(dbHash))
                {
                    passwordInDatabase = true;
                }
            }
            return passwordInDatabase;
        }

        protected bool secondPasswordInDatabase(string email, string newPassword)
        {
            var passwordInDatabase = true;
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash("second", email);
            string dbSalt = getDBSalt("second", email);
          
            string passwordWithSalt = newPassword + dbSalt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
            string userHash = Convert.ToBase64String(hashWithSalt);
            if (userHash.Equals(dbHash))
            {
                passwordInDatabase = true;
            } else
            {
                passwordInDatabase = false;
            }
            
            return passwordInDatabase;
        }

        protected bool thirdPasswordInDatabase(string email, string newPassword)
        {
            var passwordInDatabase = true;
            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash("third", email);
            string dbSalt = getDBSalt("third", email);

            string passwordWithSalt = newPassword + dbSalt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
            string userHash = Convert.ToBase64String(hashWithSalt);
            if (userHash.Equals(dbHash))
            {
                passwordInDatabase = true;
            } else
            {
                passwordInDatabase = false;
            }
            
            return passwordInDatabase;
        }

        protected string getDBHash(string passwordType, string email)
        {
            string h = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            if (passwordType == "current")
            {
                string sql = "SELECT PasswordHash FROM Account WHERE Email=@Email";
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
            }
            else if (passwordType == "second")
            {
                string sql = "SELECT SecondPasswordHash FROM Account WHERE Email=@Email";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["SecondPasswordHash"] != null)
                            {
                                if (reader["SecondPasswordHash"] != DBNull.Value)
                                { h = reader["SecondPasswordHash"].ToString(); }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { throw new Exception(ex.ToString()); }
                finally { con.Close(); }
            }
            else if (passwordType == "third")
            {
                string sql = "SELECT ThirdPasswordHash FROM Account WHERE Email=@Email";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ThirdPasswordHash"] != null)
                            {
                                if (reader["ThirdPasswordHash"] != DBNull.Value)
                                { h = reader["ThirdPasswordHash"].ToString(); }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { throw new Exception(ex.ToString()); }
                finally { con.Close(); }
            }
            return h;

        }

        protected string getDBSalt(string passwordType, string email)
        {
            string s = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            if (passwordType == "current")
            {
                string sql = "SELECT PasswordSalt FROM Account WHERE Email=@Email";
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
            }
            else if (passwordType == "second")
            {
                string sql = "SELECT SecondPasswordSalt FROM Account WHERE Email=@Email";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["SecondPasswordSalt"] != null)
                            {
                                if (reader["SecondPasswordSalt"] != DBNull.Value)
                                { s = reader["SecondPasswordSalt"].ToString(); }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { throw new Exception(ex.ToString()); }
                finally { con.Close(); }
            }
            else if (passwordType == "third")
            {
                string sql = "SELECT ThirdPasswordSalt FROM Account WHERE Email=@Email";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ThirdPasswordSalt"] != null)
                            {
                                if (reader["ThirdPasswordSalt"] != DBNull.Value)
                                { s = reader["ThirdPasswordSalt"].ToString(); }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { throw new Exception(ex.ToString()); }
                finally { con.Close(); }
            }
            
            return s;
        }
    }
}