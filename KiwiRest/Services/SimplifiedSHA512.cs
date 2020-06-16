using System;
using System.Security.Cryptography;
using System.Text;

namespace KiwiRest.Services
{
	public abstract class SHA512
	{
		public static string Hash(string message)
		{
			System.Security.Cryptography.SHA512 sha = new SHA512CryptoServiceProvider();
			return BitConverter.
				ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(message)))
				.ToLower()
				.Replace("-", "");
		}
	}
}