using System;
using System.Data;
using KiwiRest.Models;
using Npgsql;

namespace KiwiRest.Services
{
	public class Database
	{
		private static readonly string _host = Environment.GetEnvironmentVariable("postgreshost");
		private static readonly string _port = Environment.GetEnvironmentVariable("postgresport");
		private static readonly string _user = Environment.GetEnvironmentVariable("postgresuser");
		private static readonly string _password = Environment.GetEnvironmentVariable("postgrespassword");
		private static readonly string _db = Environment.GetEnvironmentVariable("postgresdb");
		
		protected static NpgsqlConnection database;

		public static void Initialize()
		{
			string connString = $"Server={_host};Username={_user};Database={_db};Port={_port};Password={_password}"; 
			
			database = new NpgsqlConnection(connString);
			database.Open();
		}

		// public static void Initialise()
		// {
		// 	string connString = $"Server={_host};Username={_user};Database={_db};Port={_port};Password={_password}"; 
		// 	database = new NpgsqlConnection(connString);
		// 	
		// 	database.Open();
		// }
	}
}