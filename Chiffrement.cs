using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeWare
{
    public class Chiffrement
    {
        public static string password = "safeware";

        private static List<byte[]> GenerateAlgotihmInputs(string password)
        {

            byte[] key;
            byte[] iv;

            List<byte[]> result = new List<byte[]>();

            Rfc2898DeriveBytes rfcDb = new Rfc2898DeriveBytes(password, System.Text.Encoding.UTF8.GetBytes(password));

            key = rfcDb.GetBytes(16);
            iv = rfcDb.GetBytes(16);

            result.Add(key);
            result.Add(iv);

            return result;
        }

        public static string Chiffre(string message, string password)
        {
            try
            {
                // Encode message and password
                byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);
                byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);

                // Set encryption settings -- Use password for both key and init. vector
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                ICryptoTransform transform = provider.CreateEncryptor(passwordBytes, passwordBytes);
                CryptoStreamMode mode = CryptoStreamMode.Write;

                // Set up streams and encrypt
                MemoryStream memStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
                cryptoStream.Write(messageBytes, 0, messageBytes.Length);
                cryptoStream.FlushFinalBlock();

                // Read the encrypted message from the memory stream
                byte[] encryptedMessageBytes = new byte[memStream.Length];
                memStream.Position = 0;
                memStream.Read(encryptedMessageBytes, 0, encryptedMessageBytes.Length);

                // Encode the encrypted message as base64 string
                string encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

                return encryptedMessage;
            }
            catch
            {
                return "";
            }
        }

        public static string Dechiffre(string encryptedMessage, string password)
        {
            try
            {
                // Convert encrypted message and password to bytes
                byte[] encryptedMessageBytes = Convert.FromBase64String(encryptedMessage);
                byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);

                // Set encryption settings -- Use password for both key and init. vector
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                ICryptoTransform transform = provider.CreateDecryptor(passwordBytes, passwordBytes);
                CryptoStreamMode mode = CryptoStreamMode.Write;

                // Set up streams and decrypt
                MemoryStream memStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
                cryptoStream.Write(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
                cryptoStream.FlushFinalBlock();

                // Read decrypted message from memory stream
                byte[] decryptedMessageBytes = new byte[memStream.Length];
                memStream.Position = 0;
                memStream.Read(decryptedMessageBytes, 0, decryptedMessageBytes.Length);

                // Encode deencrypted binary data to base64 string
                string message = Encoding.UTF8.GetString(decryptedMessageBytes);

                return message;
            }
            catch
            {
                return "";
            }
        }

        //NON FIPS
        public static string ChiffreOld(string s, string password)
        {
            try
            {
                List<byte[]> inputs = GenerateAlgotihmInputs(password);

                // Place le texte à chiffrer dans un tableau d'octets
                byte[] plainText = Encoding.UTF8.GetBytes(s);

                // Place la clé de chiffrement dans un tableau d'octets
                byte[] key = inputs[0];

                // Place le vecteur d'initialisation dans un tableau d'octets
                byte[] iv = inputs[1];

                RijndaelManaged rijndael = new RijndaelManaged
                {

                    // Définit le mode utilisé
                    Mode = CipherMode.CBC
                };

                // Crée le chiffreur AES - Rijndael
                ICryptoTransform aesEncryptor = rijndael.CreateEncryptor(key, iv);

                MemoryStream ms = new MemoryStream();

                // Ecris les données chiffrées dans le MemoryStream
                CryptoStream cs = new CryptoStream(ms, aesEncryptor, CryptoStreamMode.Write);
                cs.Write(plainText, 0, plainText.Length);
                cs.FlushFinalBlock();


                // Place les données chiffrées dans un tableau d'octet
                byte[] CipherBytes = ms.ToArray();


                ms.Close();
                cs.Close();

                // Place les données chiffrées dans une chaine encodée en Base64
                return Convert.ToBase64String(CipherBytes);
            }
            catch
            {
                return "";
            }
        }

        public static string DechiffreOld(string s, string password)
        {
            try
            {
                List<byte[]> inputs = GenerateAlgotihmInputs(password);

                // Place le texte à déchiffrer dans un tableau d'octets
                byte[] cipheredData = Convert.FromBase64String(s);

                // Place la clé de déchiffrement dans un tableau d'octets
                byte[] key = inputs[0];

                // Place le vecteur d'initialisation dans un tableau d'octets
                byte[] iv = inputs[1];

                RijndaelManaged rijndael = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                };


                // Ecris les données déchiffrées dans le MemoryStream
                ICryptoTransform decryptor = rijndael.CreateDecryptor(key, iv);
                MemoryStream ms = new MemoryStream(cipheredData);
                CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

                // Place les données déchiffrées dans un tableau d'octet
                byte[] plainTextData = new byte[cipheredData.Length];

                int decryptedByteCount = cs.Read(plainTextData, 0, plainTextData.Length);

                ms.Close();
                cs.Close();

                return Encoding.UTF8.GetString(plainTextData, 0, decryptedByteCount);
            }
            catch
            {
                return "";
            }
        }
    }
}
