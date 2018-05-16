using System.Configuration;

namespace SPServiceClient
{
    public class Configuration
    {
        public static string SiteServiceAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["WCFBaseAddress_Site"];
            }
        }

        public static string FarmServiceAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["WCFBaseAddress_Farm"];
            }
        }

        public static string Username
        {
            get
            {
                return ConfigurationManager.AppSettings["Username"];
            }
        }

        public static string UserDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["UserDomain"];
            }
        }

        public static string UserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["UserPassword"];
            }
        }

        public static System.Net.NetworkCredential Credential
        {
            get
            {
                return new System.Net.NetworkCredential(Username, UserPassword, UserDomain);
            }
        }
    }
}
