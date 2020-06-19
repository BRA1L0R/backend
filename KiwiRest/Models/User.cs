using System;
using System.Security.Claims;
using Newtonsoft.Json;

namespace KiwiRest.Models
{
	public class User
	{
		public int ID;
		public string username;
		public string email;
		public string password;	
		public DateTime registration_timestamp;
		public DateTime date_of_birth;
		public string role;
		public Plan plan;
		public bool confirmed;
		public string api_token;

		public ClaimsPrincipal ClaimsPrincipal(string authenticationType = "")
		{
			ClaimsIdentity userIdentity = new ClaimsIdentity(authenticationType);
			
			userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ID + ""));
			userIdentity.AddClaim(new Claim(ClaimTypes.Name, username));
			userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
			userIdentity.AddClaim(new Claim(ClaimTypes.UserData, plan.Name));

			return new ClaimsPrincipal(userIdentity);
		}
	}
}