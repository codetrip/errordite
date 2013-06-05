/*
 * Created by: Nick Champion
 * Created: 25 August 2008
 */

using Errordite.Core.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using Errordite.Core.Extensions;

namespace Errordite.Core.Encryption
{
    /// <summary>
    /// RijndaelSymmetricEncryptor uses the Rijndael symmetric algorithm to encrypt and decrypt data
    /// </summary>
    public class RijndaelSymmetricEncryptor : IEncryptor
    {
        private readonly IPasswordLocator _passwordLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelSymmetricEncryptor"/> class.
        /// </summary>
        /// <param name="passwordLocator">The password locator, used to locate external passwords for use with the Rijndael symmetric algorithm.</param>
        public RijndaelSymmetricEncryptor(IPasswordLocator passwordLocator)
        {
            _passwordLocator = passwordLocator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RijndaelSymmetricEncryptor"/> class.
        /// </summary>
        public RijndaelSymmetricEncryptor() : 
            this(new PresetPasswordLocator())
        { }

        #region Implementation of IEncryptor

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string Encrypt(string input)
        {
            ArgumentValidation.ComponentNotNull(_passwordLocator);

            if (input.IsNullOrEmpty())
                return string.Empty;

            Rijndael rinedal = null;
            byte[] plainText = null;
            string base64Encoded;

            try
            {
                string password = _passwordLocator.Locate();
                
                //get the ASCII bytes for the input
                plainText = Encoding.ASCII.GetBytes(input);

                //create the password derived bytes used to generate the key and IV values
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, GenerateSalt(password));

                //Instantiate the Rijndael symmetric algorithm
                rinedal = Rijndael.Create();
                rinedal.Padding = PaddingMode.ISO10126;
                rinedal.KeySize = 256;

                //create the encryptor
                ICryptoTransform cryptoTransform = rinedal.CreateEncryptor(pdb.GetBytes(32), pdb.GetBytes(16));

                //encrypt and encode to Base64
                base64Encoded = Convert.ToBase64String(cryptoTransform.TransformFinalBlock(plainText, 0, plainText.Length));
            }
            finally
            {
                // this clears out any secret data
                if (rinedal != null)
                    rinedal.Clear();

                // zeroes out our array
                if (plainText != null)
                {
                    for (int i = 0; i < plainText.Length; i++)
                        plainText[i] = 0;
                }
            }

            return base64Encoded;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string Decrypt(string input)
        {
            ArgumentValidation.ComponentNotNull(_passwordLocator);

            if (input.IsNullOrEmpty())
                return string.Empty;

            Rijndael rinedal = null;
            byte[] cipherText = Convert.FromBase64String(input);
            byte[] plainText;

            try
            {
                string password = _passwordLocator.Locate();
                
                //create the password derived bytes used to generate the key and IV values
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, GenerateSalt(password));

                //Instantiate the Rijndael symmetric algorithm
                rinedal = Rijndael.Create();
                rinedal.Padding = PaddingMode.ISO10126;

                //create the Decryptor
                ICryptoTransform cryptoTransform = rinedal.CreateDecryptor(pdb.GetBytes(32), pdb.GetBytes(16));

                //Decrypt
                plainText = cryptoTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
            }
            finally
            {
                if (rinedal != null)
                    rinedal.Clear();
            }

            return Encoding.ASCII.GetString(plainText);
        }

        /// <summary>
        /// Generates the salt.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private static byte[] GenerateSalt(string password)
        {
            return Encoding.Default.GetBytes("A salt must always be the same for Rfc2898DeriveBytes" + password);
        }

        #endregion
    }
}
