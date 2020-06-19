using System;
using System.Security.Claims;
using KiwiRest.Middlewares;
using KiwiRest.Models;
using NUnit.Framework;

namespace UnitTesting
{
	public class ModelTests
	{
		private User _testUser;
		
		[SetUp]
		public void Setup()
		{
			_testUser = new User
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
		}

		[Test]
		public void TestUserModel()
		{
			ClaimsPrincipal userClaimsPrincipal = _testUser.ClaimsPrincipal(TokenScope.UserLogin);
			
			Assert.IsTrue(userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value == _testUser.ID + "", "Is the ID in the Claims?");
			Assert.IsTrue(userClaimsPrincipal.FindFirst(ClaimTypes.Name).Value == _testUser.username, "Is the username in the Claims?");
			Assert.IsTrue(userClaimsPrincipal.FindFirst(ClaimTypes.Role).Value == _testUser.role, "Is the Role in the Claims?");
			Assert.IsTrue(userClaimsPrincipal.FindFirst(ClaimTypes.UserData).Value == _testUser.plan.Name, "Is the Plan info in the Claims?");
		}

		[Test]
		public void TestPlanModel()
		{
			Assert.IsTrue(Plans.GetPlanByName("plan.premium").Equals(Plans.Premium), "Does the function return the proper object?");
			Assert.IsTrue(Plans.GetPlanByName("plan.basic").Equals(Plans.Basic), "Does the function return the proper object?");
			Assert.IsNull(Plans.GetPlanByName("random name"), "Does a random name return a null object?");
		}
	}
}