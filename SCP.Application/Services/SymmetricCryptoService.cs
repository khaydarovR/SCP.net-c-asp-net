using Microsoft.Extensions.Options;
using SCP.Application.Common.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace SCP.Application.Services
{
    public class SymmetricCryptoService
    {
        private readonly IOptions<MyOptions> myOptions;

        public SymmetricCryptoService(IOptions<MyOptions> myOptions)
        {
            this.myOptions = myOptions;
        }

        public string EncryptWithSecretKey(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                var base64Key = DeriveAes256Key(myOptions.Value.CRT_KEY);

                aes.Key = Convert.FromBase64String(base64Key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                    }
                    array = memoryStream.ToArray();
                }
            }
            return Convert.ToBase64String(array);
        }

        public string DecryptWithSecretKey(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(myOptions.Value.CRT_KEY);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Сделать кастомный ключ валидным для алгритма aes
        /// </summary>
        /// <param name="customKey"></param>
        /// <returns></returns>
        public string DeriveAes256Key(string customKey)
        {
            // Convert the custom key to bytes
            byte[] customKeyBytes = Encoding.UTF8.GetBytes(customKey);

            // Define the salt (an arbitrary value for added security)
            byte[] salt = Encoding.UTF8.GetBytes("MySaltValue");

            // Use a key derivation function to create a valid AES-256 key
            using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(customKeyBytes, salt, 10000))
            {
                byte[] aesKey = keyDerivation.GetBytes(32); // 32 bytes for AES-256

                // Convert the derived key to a Base64 string for storage or use
                string aesKeyBase64 = Convert.ToBase64String(aesKey);

                return aesKeyBase64;
            }
        }

    }
}
