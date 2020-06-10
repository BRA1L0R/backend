using System;
using System.Security.Claims;

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
		public string plan;
		public bool confirmed;

		public ClaimsPrincipal GenerateClaimsPrincipal(string authenticationType)
		{
			ClaimsIdentity userIdentity = new ClaimsIdentity(authenticationType);
			
			userIdentity.AddClaim(new Claim(ClaimTypes.Name, username));
			userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
			userIdentity.AddClaim(new Claim(ClaimTypes.UserData,plan));

			return new ClaimsPrincipal(userIdentity);
		}
	}
}