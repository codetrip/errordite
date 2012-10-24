namespace CodeTrip.Core.Encryption
{
    public interface IEncryptor
    {
        /// <summary>
        /// Decrypts a string 
        /// </summary>
        /// <param name="cypherText"></param>
        /// <returns>the decrypted value of the string</returns>
        string Decrypt(string cypherText);

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <returns>the encrypted value of the string</returns>
        string Encrypt(string plainText);
    }
}