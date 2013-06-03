using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Errordite.Core.Encryption
{
    public class TripleDesEncryptor : IEncryptor
    {
        private const string EncryptionKey = "ENTERAKEYHERE";

        /// <summary>
        /// Decrypts a string 
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns>the decrypted value of the string</returns>
        public string Decrypt(string encryptedString)
        {
            if (string.IsNullOrEmpty(encryptedString)) return String.Empty;

            try
            {
                using (TripleDESCryptoServiceProvider cypher = new TripleDESCryptoServiceProvider())
                {
                    PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptionKey, new byte[0]);
                    cypher.Key = pdb.GetBytes(16);
                    cypher.IV = pdb.GetBytes(8);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, cypher.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            byte[] data = Convert.FromBase64String(encryptedString);
                            cs.Write(data, 0, data.Length);
                            cs.Close();

                            return Encoding.Unicode.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <returns>the encrypted value of the string</returns>
        public string Encrypt(string decryptedString)
        {
            if (String.IsNullOrEmpty(decryptedString)) return String.Empty;

            using (TripleDESCryptoServiceProvider cypher = new TripleDESCryptoServiceProvider())
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptionKey, new byte[0]);

                cypher.Key = pdb.GetBytes(16);
                cypher.IV = pdb.GetBytes(8);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, cypher.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.Unicode.GetBytes(decryptedString);

                        cs.Write(data, 0, data.Length);
                        cs.Close();

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

        }
    }

}
