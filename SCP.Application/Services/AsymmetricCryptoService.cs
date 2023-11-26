using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Services
{
    public class AsymmetricCryptoService
    {
        public RSACryptoServiceProvider RSA { get; private set; }

        public AsymmetricCryptoService()
        {
            RSA = new RSACryptoServiceProvider(2048);
        }

        /// <summary>
        /// Расшифровывает данные из сейфа с помощью приватного ключа сейфа
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public string DecryptFromClientData(string encryptedText, string privateKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);  // Convert encrypted text into bytes
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.ImportFromPem(privateKey); // Setting privateKey 

                    byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, false);  //// false for PKCS#1 padding
                    string decryptedData = Encoding.UTF8.GetString(decryptedBytes);  // Convert bytes to string
                    return decryptedData;  // Return decrypted text
                }
                finally
                {
                    rsa.PersistKeyInCsp = false; // Clear rsa key container
                }
            }
        }

        /// <summary>
        /// Создает RSA ключи и экспортирует в формате pem
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public (string publicKeyPem, string privateKeyPem) GenerateKeys(int keySize = 2048)
        {
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                try
                {
                    var privateKey = rsa.ExportParameters(true);
                    var publicKey = rsa.ExportParameters(false);

                    return (PublicKey: ExportPublicKeyToPemString(publicKey),
                            PrivateKey: ExportPrivateKeyToPemString(privateKey));
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        private string ExportPublicKeyToPemString(RSAParameters publParams)
        {
            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            var publicKey = DotNetUtilities.GetRsaPublicKey(publParams);
            pemWriter.WriteObject(publicKey);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        private string ExportPrivateKeyToPemString(RSAParameters privParams)
        {
            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            var privateKey = DotNetUtilities.GetRsaKeyPair(privParams);
            pemWriter.WriteObject(privateKey);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }


        /// <summary>
        /// Шифрует данные с помощью публичного ключа клиента, выдает в b64
        /// </summary>
        /// <param name="data"></param>
        /// <param name="privateKeyPemFromClient"></param>
        /// <returns></returns>
        public string EncryptDataForClient(string data, string publicKeyPem)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportFromPem(publicKeyPem); // might need to use a library for this, like BouncyCastle
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encryptedDataBytes = rsa.Encrypt(dataBytes, false);
            return Convert.ToBase64String(encryptedDataBytes);
        }
    }
}
