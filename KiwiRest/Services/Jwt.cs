using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using KiwiRest.Models;
using Microsoft.IdentityModel.Tokens;

namespace KiwiRest.Services
{
	public abstract class Jwt
	{
		private static byte[] key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("jwtkey") ?? throw new Exception("EMPTY_KEY_JWT"));
		
		public static string Sign(Account account, Scope scope, int expirationMinutes = 20)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor()
			{
				Subject = new ClaimsIdentity(new []
				{
					new Claim(ClaimTypes.Name, account.Username), 
					new Claim(ClaimTypes.Version, scope.ToString())
				}),
				Expires = DateTime.Now.AddMinutes(expirationMinutes),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
			};

			var stoken = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(stoken);
		}

		public static bool Validate(string token, out string username, out Scope scope)
		{
			username = null;
			scope = Scope.Registration;

			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var verifiedToken = tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidateIssuer = false,
					ValidateAudience = false,
					IssuerSigningKey = new SymmetricSecurityKey(key)
				}, out SecurityToken securityToken);

				username = verifiedToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
				if(!Scope.TryParse(verifiedToken.Claims.First(claim => claim.Type == ClaimTypes.Version).Value, out Scope decodedScope)) throw new Exception("INVALID_SCOPE");

				scope = decodedScope;
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}