using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BeetleX.Redis;
using KiwiRest.Middlewares;
using KiwiRest.Models;
using Microsoft.AspNetCore.Mvc;

namespace KiwiRest.Controllers
{
	[ApiController]
	public class DatabaseApi : ControllerBase
	{
		private static async Task<(RedisDB, int)> GetDb(int id)
		{
			RedisDB db = new RedisDB(id, new JsonFormater());

			string redisHost = Environment.GetEnvironmentVariable("redishost") ?? throw new Exception("NO_REDIS_HOST_ENV");
			int redisPort = Int32.Parse(Environment.GetEnvironmentVariable("redisport") ?? throw new Exception("NO_REDIS_PORT_ENV"));
			string redisPassword = Environment.GetEnvironmentVariable("redispassword") ?? ""; // password is not necessary to login a redis db unless a user explicitly specified

			db.Host.AddWriteHost(redisHost, redisPort).Password = redisPassword;

			var keyspace = (await db.Info()).Keyspace;
			var entries = 0;
			if (!keyspace.ContainsKey($"db{id}")) goto End;
			
			var data = keyspace[$"db{id}"];
			entries = Int32.Parse(data.Split(',')[0].Split('=')[1]);

			End:
			return (db, entries);
		}

		private IActionResult ValidateToken(out int userId)
		{
			userId = -1;
			
			if (!HttpContext.User.Identity.IsAuthenticated)
				return Unauthorized("Invalid JWT token");
			if (HttpContext.User.Identity.AuthenticationType != TokenScope.DatabaseAccess)
				return Unauthorized("Wrong token scope");

			userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			return null;
		}
		
		[HttpGet("/db/{key}")]
		public async Task<IActionResult> GetValue(string key)
		{
			IActionResult authenticated = ValidateToken(out int userId);
			if (authenticated != null) return authenticated;
			
			(RedisDB db, int entries) = await GetDb(userId);
			
			string value = await db.Get<string>(key); // get it? because it's a key-value storage hah
			return value == null ? (IActionResult) NotFound() : Ok(value);
		}

		[HttpDelete("/db/{key}")]
		public async Task<IActionResult> DeleteValue(string key)
		{
			IActionResult authenticated = ValidateToken(out int userId);
			if (authenticated != null) return authenticated;

			(RedisDB db, int entries) = await GetDb(userId);
			long amountDeleted = await db.Del(key);

			return amountDeleted > 0 ? Ok() : (IActionResult) NotFound();
		}

		[HttpPost("/db/{key}")]
		public async Task<IActionResult> SetValue(string key, [FromForm] string value)
		{
			IActionResult authenticated = ValidateToken(out int userId);
			if (authenticated != null) return authenticated;
			
			(RedisDB db, int entries) = await GetDb(userId);

			var plan = Plans.GetPlanByName(HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.UserData).Value) ?? throw new Exception("Unknown plan type");
			if (plan.MaxEntries <= entries && await db.Exists(key) == 0) return Conflict("Too many entries for current plan");

			await db.Set(key, value);
			return Ok();
		}
	}
}