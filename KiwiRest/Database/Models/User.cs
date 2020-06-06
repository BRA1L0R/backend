using System;

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
	}
}