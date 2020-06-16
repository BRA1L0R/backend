using System;
using System.Text;

namespace KiwiRest.Services
{
	public abstract class StringSanitization
	{
		public static string Sanitize(string str) {
			StringBuilder sb = new StringBuilder();
			if (String.IsNullOrEmpty(str)) return "";
			foreach (char c in str) {
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '@') {
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
	}
}