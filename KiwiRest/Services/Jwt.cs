using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using KiwiRest.Models;
using Microsoft.IdentityModel.Tokens;

namespace KiwiRest.Services
{
	public abstract class Jwt
	{
		private static byte[] key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("jwtkey") ?? throw new Exception("EMPTY_KEY_JWT"));

		// public static string Sign(string username, Scope scope, int expirationMinutes = 20)
		// {
		// 	return Sign(new User {username = username}, scope, expirationMinutes);
		// }
		// public static string Sign(User user, Scope scope, int expirationMinutes = 20)
		// {
		// 	var tokenHandler = new JwtSecurityTokenHandler();
		// 	var tokenDescriptor = new SecurityTokenDescriptor()
		// 	{
		// 		Subject = new ClaimsIdentity(new []
		// 		{
		// 			new Claim(ClaimTypes.Name, user.username), 
		// 			new Claim(ClaimTypes.Version, scope.ToString())
		// 		}),
		// 		Expires = DateTime.Now.AddMinutes(expirationMinutes),
		// 		SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
		// 	};
		//
		// 	var stoken = tokenHandler.CreateToken(tokenDescriptor);
		// 	return tokenHandler.WriteToken(stoken);
		// }

		// public static bool Validate(string token, out string username, out Scope scope)
		// {
		// 	username = null;
		// 	scope = Scope.Registration;
		//
		// 	var tokenHandler = new JwtSecurityTokenHandler();
		// 	try
		// 	{
		// 		var verifiedToken = tokenHandler.ValidateToken(token, new TokenValidationParameters
		// 		{
		// 			ValidateLifetime = true,
		// 			ValidateIssuerSigningKey = true,
		// 			ValidateIssuer = false,
		// 			ValidateAudience = false,
		// 			IssuerSigningKey = new SymmetricSecurityKey(key)
		// 		}, out SecurityToken securityToken);
		//
		// 		username = verifiedToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
		// 		if(!Scope.TryParse(verifiedToken.Claims.First(claim => claim.Type == ClaimTypes.Version).Value, out Scope decodedScope)) throw new Exception("INVALID_SCOPE");
		//
		// 		scope = decodedScope;
		// 		return true;
		// 	}
		// 	catch (Exception)
		// 	{
		// 		return false;
		// 	}
		// }

		public static string Sign(ClaimsIdentity identity, int expirationMinutes = 20)
		{
			var securityKey = new SymmetricSecurityKey(key);
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			
			// I guess i have to to it in this way :/
			identity.AddClaim(new Claim(ClaimTypes.Authentication, identity.AuthenticationType));

			var token = new JwtSecurityToken(
				issuer: "Kiwi",
				audience: "Authentication",
				claims: identity.Claims,
				expires: DateTime.Now.AddMinutes(expirationMinutes),
				signingCredentials: credentials
			);
			
			var tokenHandler = new JwtSecurityTokenHandler();
			var serializedToken = tokenHandler.WriteToken(token);
			return serializedToken;
		}

		public static bool Validate(string token, out ClaimsIdentity claimsIdentity)
		{
			claimsIdentity = null;
			var tokenHandler = new JwtSecurityTokenHandler();

			try
			{
				var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidateIssuer = false,
					ValidateAudience = false,
					IssuerSigningKey = new SymmetricSecurityKey(key)
				}, out SecurityToken securityToken);
				
				// Decode the authentication type from the claims, only way to bring it with the jwt token :/
				var authenticationType = claimsPrincipal.FindFirst(claim => claim.Type == ClaimTypes.Authentication);
				claimsIdentity = new ClaimsIdentity(claimsPrincipal.Claims, claimsPrincipal.Claims.First(claim => claim.Type == ClaimTypes.Authentication).Value); // well...
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}