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

        public string DecryptData(string encryptedText, string privateKey)
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


        public (string publicKey, string privateKey) GenerateKeys(int keySize = 2048)
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

        public string ExportPublicKeyToPemString(RSAParameters publParams)
        {
            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            var publicKey = DotNetUtilities.GetRsaPublicKey(publParams);
            pemWriter.WriteObject(publicKey);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        public string ExportPrivateKeyToPemString(RSAParameters privParams)
        {
            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            var privateKey = DotNetUtilities.GetRsaKeyPair(privParams);
            pemWriter.WriteObject(privateKey);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }


    }
}
