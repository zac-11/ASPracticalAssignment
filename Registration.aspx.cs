using System;
using System.Collections.Generic;
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

namespace PracticalAssignment
{
    public partial class Registration : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Register_Click(object sender, EventArgs e)
        {
            string password = HttpUtility.HtmlEncode(tb_password.Text.ToString().Trim());

            //Generate random "salt" 
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];

            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            salt = Convert.ToBase64String(saltByte);

            SHA512Managed hashing = new SHA512Managed();

            string pwdWithSalt = password + salt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(password));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

            finalHash = Convert.ToBase64String(hashWithSalt);

            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;

            createAccount();
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

        protected void createAccount()
        {
            if (validateOtherInput() is true) 
            { 
                try
                {
                    using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@FirstName,@LastName,@CreditCardNumber,@Email,@PasswordHash,@PasswordSalt,@DateTimeRegistered,@DateOfBirth,@Photo,@IV,@Key,@LockoutCount,@DateTimeLockout)"))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@FirstName", HttpUtility.HtmlEncode(tb_firstName.Text.Trim()));
                                cmd.Parameters.AddWithValue("@LastName", HttpUtility.HtmlEncode(tb_lastName.Text.Trim()));
                                cmd.Parameters.AddWithValue("@CreditCardNumber", encryptData(HttpUtility.HtmlEncode(tb_creditCardInfo.Text.Trim())));
                                cmd.Parameters.AddWithValue("@Email", HttpUtility.HtmlEncode(tb_email.Text.Trim()));
                                cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                                cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                                cmd.Parameters.AddWithValue("@DateTimeRegistered", DateTime.Now);
                                cmd.Parameters.AddWithValue("@DateOfBirth", HttpUtility.HtmlEncode(tb_dateOfBirth.Text.Trim()));
                                cmd.Parameters.AddWithValue("@Photo", uploadPhoto.FileContent);
                                cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                                cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                                cmd.Parameters.AddWithValue("@LockoutCount", 0);
                                cmd.Parameters.AddWithValue("@DateTimeLockout", DateTime.Now);
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
                Response.Redirect("Login.aspx", false);
            }
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

        protected Boolean validateOtherInput()
        {
            Boolean isValid = true;
            // check input isEmpty
            if (HttpUtility.HtmlEncode(tb_firstName.Text) == "")
            { lbl_firstNameChecker.Text = "First name cannot be empty"; lbl_firstNameChecker.ForeColor = Color.Red; isValid = false; }

            if (HttpUtility.HtmlEncode(tb_lastName.Text) == "")
            { lbl_lastNameChecker.Text = "Last name cannot be empty"; lbl_lastNameChecker.ForeColor = Color.Red; isValid = false; }

            if (HttpUtility.HtmlEncode(tb_creditCardInfo.Text).Length != 16)
            { lbl_creditCardChecker.Text = "Credit card requires 16 digits"; lbl_creditCardChecker.ForeColor = Color.Red; isValid = false; }

            var emailInDatabase = false;
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString)) {
                    SqlCommand cmd = new SqlCommand("SELECT Email FROM Account;", con);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0).Trim() == HttpUtility.HtmlEncode(tb_email.Text.ToString().Trim()))
                            {
                                lbl_emailChecker.Text = "Email is used, please provide another email.";
                                lbl_emailChecker.ForeColor = Color.Red;
                                emailInDatabase = true;
                            }
                        }
                        reader.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception)
            {
                //throw new Exception(ex.ToString());
                lbl_emailChecker.Text = "unknown error"; lbl_emailChecker.ForeColor = Color.Red;
            }

            if (emailInDatabase is true) { isValid = false; }
            if (!Regex.IsMatch(HttpUtility.HtmlEncode(tb_email.Text.ToString().Trim()), @"^\w+[\+\.\w-]*@([\w-]+\.)*\w+[\w-]*\.([a-z]{2,4}|\d+)$"))
            { lbl_emailChecker.Text = "Invalid email"; lbl_emailChecker.ForeColor = Color.Red; isValid = false; }

            if (HttpUtility.HtmlEncode(tb_dateOfBirth.Text) == "")
            { lbl_dateOfBirthChecker.Text = "Date of Birth cannot be empty"; lbl_dateOfBirthChecker.ForeColor = Color.Red; isValid = false; }

            return isValid;
        }
        
        protected void onPasswordTextChanged(object sender, EventArgs e)
        {
            int scores = checkPassword(HttpUtility.HtmlEncode(tb_password.Text));
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
    }
}