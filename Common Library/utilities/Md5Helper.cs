using System;
using System.Security.Cryptography;
using System.Text;

namespace hp.utilities
{
    public class Md5Helper
    {
        public static string Encrypt(string ToEncrypt, string Key)
        {
            try
            {
                MD5CryptoServiceProvider Md5Provider = new MD5CryptoServiceProvider();
                byte[] ToEncryptBypeArray = Encoding.UTF8.GetBytes(ToEncrypt);
                byte[] KeyBypeArray = Md5Provider.ComputeHash(Encoding.UTF8.GetBytes(Key));
                Md5Provider.Clear();

                TripleDESCryptoServiceProvider CryptoProvider = new TripleDESCryptoServiceProvider();
                CryptoProvider.Key = KeyBypeArray;
                CryptoProvider.Mode = CipherMode.ECB;
                CryptoProvider.Padding = PaddingMode.PKCS7;

                ICryptoTransform CryptoTransform = CryptoProvider.CreateEncryptor();
                byte[] ReturnByteArray = CryptoTransform.TransformFinalBlock(ToEncryptBypeArray, 0,
                                                                             ToEncryptBypeArray.Length);
                CryptoProvider.Clear();

                return Convert.ToBase64String(ReturnByteArray, 0, ReturnByteArray.Length);
            }
            catch
            {
                return "";
            }
        }

        public static string Decrypt(string ToDecrypt, string Key)
        {
            try
            {
                MD5CryptoServiceProvider Md5Provider = new MD5CryptoServiceProvider();
                byte[] ToDecryptBypeArray = Convert.FromBase64String(ToDecrypt);
                byte[] KeyByteArray = Md5Provider.ComputeHash(Encoding.UTF8.GetBytes(Key));
                Md5Provider.Clear();

                TripleDESCryptoServiceProvider CryptoProvider = new TripleDESCryptoServiceProvider();
                CryptoProvider.Key = KeyByteArray;
                CryptoProvider.Mode = CipherMode.ECB;
                CryptoProvider.Padding = PaddingMode.PKCS7;

                ICryptoTransform CryptoTransform = CryptoProvider.CreateDecryptor();
                byte[] ReturnByteArray = CryptoTransform.TransformFinalBlock(ToDecryptBypeArray, 0,
                                                                             ToDecryptBypeArray.Length);
                CryptoProvider.Clear();

                return Encoding.UTF8.GetString(ReturnByteArray);
            }
            catch
            {
                return "";
            }
        }
    }
}
