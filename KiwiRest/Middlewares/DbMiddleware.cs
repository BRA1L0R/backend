using System.Security.Claims;
using System.Threading.Tasks;
using KiwiRest.Models;
using KiwiRest.Services;
using Microsoft.AspNetCore.Http;

namespace KiwiRest.Middlewares
{
	public class ApiMiddleware
	{
		private readonly RequestDelegate next;

		public ApiMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var authHeader = context.Request.Headers["Authorization"];
			if (authHeader.Count < 1) goto Next;

			var bearer = authHeader[0].Split(' ');
			if (bearer.Length != 2) goto Next;

			var token = bearer[1];
			User user = UserDatabase.GetUser(token);
			
			if (user == null) goto Next;
			ClaimsPrincipal claimsPrincipal = user.ClaimsPrincipal();
			context.User = claimsPrincipal;
			
			Next:
			await next.Invoke(context);
		}
	}
}