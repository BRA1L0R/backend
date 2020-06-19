using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KiwiRest.Middlewares;
using KiwiRest.Models;
using KiwiRest.Services;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace UnitTesting
{
	public class MiddlewareTesting
	{
		[SetUp]
		public void Setup()
		{
			Jwt.SetKey(Encoding.UTF8.GetBytes("Very long string used to sign jwt tokens"));
		}

		[Test]
		public async Task TestJwtMiddleware()
		{
			HttpContext context = new DefaultHttpContext();

			JwtMiddleware middleware = new JwtMiddleware(httpContext => Task.CompletedTask);
			await middleware.Invoke(context);
			
			Assert.IsTrue(!context.User.Claims.Any(), "Are there any claims when there's no authentication header?");
			Assert.IsNull(context.User.Identity.AuthenticationType, "Is the authenticationtype null when there's no auth header?");
			
			// Create a test user instance
			User testUser = new User
			{
				confirmed = true,
				date_of_birth = DateTime.Now,
				email = "unit@testing.com",
				ID = 0,
				password = "hash",
				plan = Plans.Basic,
				registration_timestamp = DateTime.Now,
				role = Roles.User,
				username = "user"
			};

			ClaimsPrincipal userPrincipal = testUser.ClaimsPrincipal(TokenScope.UserLogin);
			context.Request.Headers.Add("Authorization", "Bearer " + Jwt.Sign((ClaimsIdentity) userPrincipal.Identity));

			await middleware.Invoke(context);
			
			Assert.IsTrue(context.User.Claims.Any(), "Are there any claims when the authentication header is present?");
			Assert.IsNotNull(context.User.Identity.AuthenticationType, "Is the authenticationtype no null when the authentication header is present?");
			Assert.AreEqual(context.User.Identity.AuthenticationType, TokenScope.UserLogin,
				"Is the AuthenticationType the same as the one used to sign the Jwt token?");
		}
	}
}