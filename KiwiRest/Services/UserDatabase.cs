using System;
using System.Data;
using KiwiRest.Models;
using Npgsql;

namespace KiwiRest.Services
{
	public abstract class UserDatabase : Database
	{
		public static string GetApiToken(int id)
		{
			var command = new NpgsqlCommand("SELECT \"api_token\" FROM \"Users\" WHERE \"ID\"=@id", database);
			command.Parameters.AddWithValue("id", id);

			var dr = command.ExecuteReader();
			dr.Read();

			if (!dr.HasRows)  { dr.Close(); return null; }
			
			string token = dr.GetString("api_token");
			dr.Close();

			return token;
		}
		private static User GetUser(string email, string username, int id, string apitoken)
		{
			email = StringSanitization.Sanitize(email);
			username = StringSanitization.Sanitize(username);
			apitoken = StringSanitization.Sanitize(apitoken);
			
			var command =
				new NpgsqlCommand("SELECT * FROM \"Users\" WHERE \"email\"=@email OR \"username\"=@usr OR \"ID\"=@id OR \"api_token\"=@apitoken",
					database);

			command.Parameters.AddWithValue("email", email);
			command.Parameters.AddWithValue("usr", username);
			command.Parameters.AddWithValue("id", id);
			command.Parameters.AddWithValue("apitoken", apitoken);

			var dr = command.ExecuteReader();
			dr.Read();
			
			if (!dr.HasRows) { dr.Close(); return null;}
			User user = new User
			{
				ID = dr.GetInt32("ID"),
				username = dr.GetString("username"),
				email = dr.GetString("email"),
				password = dr.GetString("password"),
				registration_timestamp = dr.GetDateTime("registration_timestamp"),
				date_of_birth = dr.GetDateTime("date_of_birth"),
				role = dr.GetString("role"),
				plan = Plans.GetPlanByName(dr.GetString("plan")) ?? throw new Exception("Unknown plan type"),
				confirmed = dr.GetBoolean("confirmed")
			};
			
			dr.Close();
			return user;
		}

		public static User GetUser(string email, string username)
		{
			return GetUser(email, username, 0, null);
		}
		
		public static User GetUser(int id)
		{
			return GetUser(null, null, id, null);
		}

		public static User GetUser(string apitoken)
		{
			return GetUser(null, null, 0, apitoken);
		}
		
		public static bool RegisterUser(User user)
		{
			var command = 
				new NpgsqlCommand("INSERT INTO \"Users\" (username, email, password, registration_timestamp, date_of_birth, role, plan, confirmed, api_token) VALUES (@usr, @email, @pwd, @reg_ts, @dob, @role, @plan, @cnf, @api)",
					database);
			command.Parameters.AddWithValue("usr", user.username);
			command.Parameters.AddWithValue("email", user.email);
			command.Parameters.AddWithValue("pwd", user.password);
			command.Parameters.AddWithValue("reg_ts", user.registration_timestamp);
			command.Parameters.AddWithValue("dob", user.date_of_birth);
			command.Parameters.AddWithValue("role", user.role);
			command.Parameters.AddWithValue("plan", user.plan.Name);
			command.Parameters.AddWithValue("cnf", user.confirmed);
			command.Parameters.AddWithValue("api", user.api_token);

			return command.ExecuteNonQuery() != 0;
		}

		public static bool ConfirmAccount(string username)
		{
			if (GetUser(null, username) == null) return false;
			var command = new NpgsqlCommand("UPDATE \"Users\" SET confirmed=true WHERE username=@usr", database);
			command.Parameters.AddWithValue("usr", username);

			if (command.ExecuteNonQuery() > 0) return true;
			return false;
		}
	}
}