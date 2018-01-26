using System;
using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace sfwctrlTache
{
	public class Chiffrement
	{
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

		public static string chiffre(string s, string password)
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

				RijndaelManaged rijndael = new RijndaelManaged();

				// Définit le mode utilisé
				rijndael.Mode = CipherMode.CBC;

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

		public static string dechiffre(string s, string password)
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

				RijndaelManaged rijndael = new RijndaelManaged();
				rijndael.Mode = CipherMode.CBC;


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
