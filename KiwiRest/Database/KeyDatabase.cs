using System;
using BeetleX.Redis;

namespace KiwiRest.Services
{
	public abstract class KeyDatabase
	{
		public static void Initialise()
		{
			Redis.Default.DataFormater = new JsonFormater();

			{
				string redisPassword = Environment.GetEnvironmentVariable("redispassword");
				if (!String.IsNullOrEmpty(redisPassword))
				{
					Redis.Default.Host.AddWriteHost(
						Environment.GetEnvironmentVariable("redishost"),
						Int32.Parse(
							Environment.GetEnvironmentVariable("redisport") ?? throw new Exception("INVALID_REDIS_PORT"))).Password = redisPassword;
				}
				else
				{
					Redis.Default.Host.AddWriteHost(
						Environment.GetEnvironmentVariable("redishost"),
						Int32.Parse(
							Environment.GetEnvironmentVariable("redisport") ?? throw new Exception("INVALID_REDIS_PORT")));
				}
			}
		}
		
		
	}
}