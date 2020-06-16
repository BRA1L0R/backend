using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using KiwiRest.Models;
using KiwiRest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace KiwiRest.Middlewares
{
	public class JwtMiddleware
	{
		private readonly RequestDelegate next;
		public JwtMiddleware(RequestDelegate next)
		{
			this.next = next;
		}
		
		public async Task Invoke(HttpContext context)
		{
			context.Response.OnStarting(() =>
			{
				// Generate new JWT if the user was already authenticated
				if (!String.IsNullOrEmpty(context.User.Identity.AuthenticationType))
					context.Response.Headers.Add("X-Token", Jwt.Sign(context.User.Identity as ClaimsIdentity));

				return Task.CompletedTask;
			});
			
			// Check if the user has a JWT token and authenticate him if so
			// if (CheckJwt(context.Request.Headers["Authorization"], out string username))
			// {
			// 	context.Response.Headers.Add("X-Token", Jwt.Sign(username, Scope.Authentication));
			// 	await next.Invoke(context);
			// }
			// else
			// 	context.Response.StatusCode = 401;/

			var authHeader = context.Request.Headers["Authorization"];
			if (authHeader.Count < 1) goto Next;

			var bearer = authHeader[0].Split(' ');
			if (bearer.Length != 2) goto Next;

			var token = bearer[1];
			if (Jwt.Validate(token, out ClaimsIdentity claimsIdentity))
				context.User = new ClaimsPrincipal(claimsIdentity);

			Next:
			await next.Invoke(context);
		}

		// private bool CheckJwt(StringValues authHeader, out string validatedUsername)
		// {
		// 	validatedUsername = null;
		// 	
		// 	if (authHeader.Count < 1) return false;
		// 	if (String.IsNullOrEmpty(authHeader[0])) return false;
		// 	if (!authHeader[0].StartsWith("Bearer")) return false;
		//
		// 	var bearer = authHeader[0].Split(' ');
		// 	if (bearer.Length != 2) return false;
		//
		// 	var token = bearer[1];
		// 	bool validate = Jwt.Validate(token, out string username, out Scope scope);
		//
		// 	validatedUsername = username;
		// 	return validate && scope == Scope.Authentication;
		// }
	}
}