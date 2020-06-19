using System;
using System.Linq;

namespace KiwiRest.Services
{
	public abstract class StringGeneration
	{
		private static readonly Random Rand = new Random();
		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[Rand.Next(s.Length)]).ToArray());
		}
	}
}
