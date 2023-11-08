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

        public string Encrypt(string plaintext, string publicKey)
        {
            RSA.FromXmlString(publicKey);
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            return Convert.ToBase64String(RSA.Encrypt(plaintextBytes, false));
        }

        public string Decrypt(string encryptedText, string privateKey)
        {
            RSA.FromXmlString(privateKey);
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            return Encoding.UTF8.GetString(RSA.Decrypt(encryptedBytes, false));
        }
    }
}
