using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using KiwiRest.Services;

namespace UnitTesting
{
	public class ServiceTesting
	{
		[SetUp]
		public void Setup()
		{
			Jwt.SetKey(Encoding.UTF8.GetBytes("very long string used to sign jwt"));
		}

		[Test]
		public void TestJwtService()
		{
			IEnumerable<Claim> testClaims = new[] {new Claim("test_key", "test_value")};
			ClaimsIdentity testIdentity = new ClaimsIdentity(testClaims, "authentication");

			string token = Jwt.Sign(testIdentity);
			Assert.IsNotNull(token, "token != null");
			Assert.IsNotEmpty(token, "token != empty");

			bool verified = Jwt.Validate(token, out testIdentity);
			Assert.AreEqual(testIdentity.AuthenticationType, "authentication", "Is the AuthenticationType correct?");
			Assert.IsTrue(testIdentity.HasClaim("test_key", "test_value"), "Does the verified output contain the claim?");
			Assert.IsTrue(verified, "Is the token verified?");
		}

		[TestCase("contamination{}/.??[]!@_'")]
		public void TestStringSanitization(string test)
		{
			string sanitize = StringSanitization.Sanitize(test);
			// string sanitize = test;
			Regex rgx = new Regex(@"^[a-zA-Z0-9\\_\.\@]*$");
			
			Assert.IsTrue(rgx.IsMatch(sanitize), "Check if the string is correctly sanitized");
		}
	}
}