using System;
using System.Security.Cryptography;
using System.Text;

namespace jy.utilities
{
    public static class Md5
    {
        public const string SHAREPOINT = "SharePoint";
        public const string PREFIX = "encrypted::";

        public static string Encrypt(this string self, string key, string prefex = "")
        {
            if (string.IsNullOrEmpty(self))
                return string.Empty;

            if (string.IsNullOrEmpty(key))
                return self;

            try
            {
                using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
                {
                    cryptoServiceProvider.Key = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key));
                    cryptoServiceProvider.Mode = CipherMode.ECB;
                    cryptoServiceProvider.Padding = PaddingMode.PKCS7;

                    var cryptoTransform = cryptoServiceProvider.CreateEncryptor();
                    var toEncryptBypeArray = Encoding.UTF8.GetBytes(self);

                    var returnByteArray = cryptoTransform.TransformFinalBlock(toEncryptBypeArray, 0, toEncryptBypeArray.Length);

                    cryptoServiceProvider.Clear();

                    return prefex + Convert.ToBase64String(returnByteArray, 0, returnByteArray.Length);
                }
            }
            catch
            {
                return prefex + string.Empty;
            }
        }

        public static string Decrypt(this string self, string key, string prefix = "")
        {
            if (string.IsNullOrEmpty(self))
                return string.Empty;
            if (string.IsNullOrEmpty(key))
                return self;

            if (!string.IsNullOrEmpty(prefix))
            {
                if (self.StartsWith(prefix))
                    self = self.Substring(prefix.Length);
            }

            try
            {
                using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
                {
                    cryptoServiceProvider.Key = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key));
                    cryptoServiceProvider.Mode = CipherMode.ECB;
                    cryptoServiceProvider.Padding = PaddingMode.PKCS7;

                    var cryptoTransform = cryptoServiceProvider.CreateDecryptor();
                    var toDecryptBypeArray = Convert.FromBase64String(self);

                    var returnByteArray = cryptoTransform.TransformFinalBlock(toDecryptBypeArray, 0, toDecryptBypeArray.Length);
                    cryptoServiceProvider.Clear();

                    return Encoding.UTF8.GetString(returnByteArray);
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
