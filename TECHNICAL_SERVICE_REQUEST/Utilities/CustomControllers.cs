using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using TECHNICAL_SERVICE_REQUEST.Models;
//using DENGUE_SYSTEM.Controllers;
using System.Web.Mvc;
using System.Net;

namespace Custom.Controllers
{
    //[Authorize]
    //public static class dbEmployee
    //{
    //    public static Registration RegistrationIdentity(string UserIdentityName)
    //    {
    //        ApplicationDbContext db = new ApplicationDbContext();
    //        return db.Registrations.Single(i => i.UserName == UserIdentityName);
    //    }

    //    public static List<UserPrivilege> UserPrivilegeNow()
    //    {
    //        ApplicationDbContext db = new ApplicationDbContext();
    //        Registration registration = db.Registrations.Single(i => i.UserName == HttpContext.Current.User.Identity.Name);

    //        var user = db.UserPrivileges.Where(i => i.RegistrationId == registration.Id).ToList();
    //        return user;
    //    }


    //    [Authorize]
    //    public static Registration RegistrationNow()
    //    {
    //        try
    //        {
    //            ApplicationDbContext db = new ApplicationDbContext();
    //            return db.Registrations.Single(i => i.UserName == HttpContext.Current.User.Identity.Name);
    //        }
    //        catch
    //        {
    //            var reg = new Registration();
    //            reg.Id = 0;
    //            return reg;
    //        }
    //    }

    //}

    public static class EncryptionHelper
    {
        private static string EncryptionKey = "DOHWISHEncryptDecrypt";  // Do not change once started

        public static string Encrypt(string clearText)
        {
            if (clearText == null || clearText == "")
            {
                return "";
            }

            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            if (cipherText == null || cipherText == "")
            {
                return "";
            }

            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }
    }


    //public static class HardCodedDatabase
    //{
    //    public static DataTable Query(string querystr)
    //    {
    //        return Query(querystr, "");
    //    }

    //    public static DataTable Query(string querystr, string connectionString)
    //    {
    //        if (connectionString == "")
    //        {
    //            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    //        }

    //        var dt = new DataTable();
    //        using (SqlConnection sqlConn = new SqlConnection(connectionString.Replace("\r\n", "")))
    //        {
    //            using (SqlCommand cmd = new SqlCommand(querystr, sqlConn))
    //            {
    //                SqlDataAdapter da = new SqlDataAdapter(cmd);
    //                da.Fill(dt);
    //            }
    //        }

    //        return dt;

            //SqlConnection cmdconn = new SqlConnection();
            //var dt = new DataTable();
            //try
            //{
            //    if (connectionString == "")
            //    {
            //        connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //    }

            //    cmdconn.ConnectionString = connectionString.Replace("\r\n", "");
            //    cmdconn.Open();

            //    SqlCommand cmd = new SqlCommand();
            //    cmd.Connection = cmdconn;
            //    cmd.CommandText = querystr;
            //    //cmd.ExecuteNonQuery();
            //    SqlDataAdapter da = new SqlDataAdapter();
            //    da.SelectCommand = cmd;
            //    da.Fill(dt);
            //}
            //catch (Exception e)
            //{
            //    //MessageBox.Show(e.Message, "ERROR!");
            //}
            //finally
            //{
            //    cmdconn.Close();
            //}
            //return dt;
    //    }
    //}

    //public static class Dates
    //{
    //    public static int Age(DateTime DateStarted, DateTime DateEnded)
    //    {
    //        int age = (Int32.Parse(DateEnded.ToString("yyyyMMdd")) - Int32.Parse(DateStarted.ToString("yyyyMMdd"))) / 10000;
    //        return age;
    //    }

    //    public static int Age(DateTime DateStarted)
    //    {
    //        return Age(DateStarted, DateTime.Now);
    //    }
    //}


    //public static class Gender
    //{
    //    public static bool Male = true;
    //    public static bool Female = false;

    //    public static string Sex(bool sex)
    //    {
    //        return (sex == true) ? "Male" : "Female";
    //    }
    //}

    //public static class strings
    //{
    //    public static string UppercaseWords(string value)
    //    {
    //        if (value == null || value.Trim() == "")
    //        {
    //            return "";
    //        }

    //        var val2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    //        return val2.Replace(" ii ", " II ").Replace(" iii ", " III ");

            //if (value == null) { return ""; }
            //value = value.Trim();
            //if (value == "") { return ""; }

            //value = value.ToLower();
            //char[] array = value.ToCharArray();
            //if (array.Length >= 1)
            //{
            //    array[0] = char.ToUpper(array[0]);

            //    for (int i = 1; i < array.Length; i++)
            //    {
            //        if (array[i - 1] == ' ')
            //        {
            //            array[i] = char.ToUpper(array[i]);
            //        }
            //    }
            //}


            //return new string(array);
    //    }

    //    public static string NoComma(string value)
    //    {
    //        return value.Replace(",", "");
    //    }
    //}
}


public class FTPHelper
{
    //var contents = GetFtpDirectoryContents(new Uri("ftpDirectoryUri"), new NetworkCredential("userName", "password"));

    //public static string Upload(string folderpath, string sourcepath)
    //{
    //    var basepath = NCROWebsite.FtpUrl + NCROWebsite.DocumentsFolder;
    //    return Upload(NCROWebsite.FtpCredential, basepath, folderpath, sourcepath);
    //}
    //public static string Upload(NetworkCredential networkCredential, string basepath, string folderpath, string sourcepath)
    //{
    //    var fp = folderpath.Split('/');

    //    try
    //    {
    //        WebRequest request = WebRequest.Create(basepath + "/" + fp[0]);
    //        request.Method = WebRequestMethods.Ftp.MakeDirectory;
    //        request.Credentials = networkCredential;
    //        WebResponse response = request.GetResponse();
    //    }
    //    catch { }

    //    try
    //    {
    //        WebRequest request = WebRequest.Create(basepath + "/" + folderpath);
    //        request.Method = WebRequestMethods.Ftp.MakeDirectory;
    //        request.Credentials = networkCredential;
    //        WebResponse response = request.GetResponse();
    //    }
    //    catch { }

    //    var filename = Path.GetFileName(sourcepath);
    //    Uri requestUri = new Uri(basepath + "/" + folderpath + "/" + filename);
    //    using (WebClient client = new WebClient())
    //    {
    //        client.Credentials = networkCredential;
    //        client.UploadFile(requestUri, "STOR", sourcepath);
    //    }
    //    File.Delete(sourcepath);

    //    return "";
    //}

    //public static List<string> GetFtpDirectoryContents(Uri requestUri, NetworkCredential networkCredential)
    //{

    //    var directoryContents = new List<string>(); // Create empty list to fill it later.
    //                                                // Create ftpWebRequest object with given options to get the Directory Contents. 

    //    var ftpWebRequest = GetFtpWebRequest(requestUri, networkCredential, WebRequestMethods.Ftp.ListDirectory);
    //    try
    //    {
    //        using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse()) // Excute the ftpWebRequest and Get It's Response.
    //        using (var streamReader = new StreamReader(ftpWebResponse.GetResponseStream())) // Get list of the Directory Contentss as Stream.
    //        {
    //            var line = string.Empty; // Initial default value for line.
    //            do
    //            {
    //                line = streamReader.ReadLine(); // Read current line of Stream.
    //                directoryContents.Add(line); // Add current line to Directory Contentss List.
    //            } while (!string.IsNullOrEmpty(line)); // Keep reading while the line has value.
    //        }
    //    }
    //    catch (Exception) { } // Do nothing incase of Exception occurred.

    //    return directoryContents; // Return all list of Directory Contentss: Files/Sub Directories.
    //}

    //public static FtpWebRequest GetFtpWebRequest(Uri requestUri, NetworkCredential networkCredential, string method = null)
    //{
    //    var ftpWebRequest = (FtpWebRequest)WebRequest.Create(requestUri); // Create FtpWebRequest with given Request Uri.
    //    ftpWebRequest.Credentials = networkCredential; //Set the Credentials of current FtpWebRequest.

    //    if (!string.IsNullOrEmpty(method))
    //    {
    //        ftpWebRequest.Method = method; // Set the Method of FtpWebRequest incase it has a value.
    //    }

    //    return ftpWebRequest; // Return the configured FtpWebRequest.
    //}
}


public class NCROWebsite
{
    // public static string DomainUrl = "localhost:26333";
    public static string DomainUrl = "ncroffice.doh.gov.ph";
    public static string HttpUrl = "http://" + DomainUrl + httpport;
    public static string FtpUrl = "ftp://" + DomainUrl;
    public static string FtpUsername = "DOH";
    public static string FtpPassword = "P@ssw0rdkmits";
    public static NetworkCredential FtpCredential = new NetworkCredential(FtpUsername, FtpPassword);

    public static bool isDemo
    {
        get
        {
            string WebsiteConnection = System.Configuration.ConfigurationManager.ConnectionStrings["NCROWebsiteConnection"].ConnectionString;
            if (WebsiteConnection.Contains("DEMO") || WebsiteConnection.Contains("test"))
            {
                return true;
            }

            return false;
        }
    }

    public static string DocumentsFolder
    {
        get
        {
            if (isDemo)
            {
                return "/NCRO_WEBSITE_TEST/DocumentsFolder";
            }

            return "/DOH_WEBSITE/DocumentsFolder";
        }
    }
    private static string httpport
    {
        get
        {

            if (isDemo)
            {
                return ":85";
            }

            return "";
        }
    }
}