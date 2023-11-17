using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Common.Helpers
{
    public class RSAUtils
    {
        public static bool VerifySignature(string publicKey, string data, string signature)
        {
            byte[] byteSignature = Convert.FromBase64String(signature);

            using (var rsa = new RSACryptoServiceProvider(2048)) // or another appropriate key size
            {
                try
                {
                    rsa.FromXmlString(publicKey);
                    var sha256 = new SHA256Managed();
                    var dataBytes = Encoding.Unicode.GetBytes(data);
                    bool isValid = rsa.VerifyData(dataBytes, sha256, byteSignature);
                    return isValid;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false; // so that we do not create additional copies of this key
                }
            }
        }

    }
}
