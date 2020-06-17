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
		private static byte[] key;
		public static void SetKey(byte[] _key)
		{
			key = _key;
		}

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

		public static bool Validate(string token, out ClaimsIdentity claimsIdentity, bool validateLifetime = true)
		{
			claimsIdentity = null;
			var tokenHandler = new JwtSecurityTokenHandler();

			try
			{
				var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateLifetime = validateLifetime,
					ValidateIssuerSigningKey = true,
					ValidateIssuer = false,
					ValidateAudience = false,
					IssuerSigningKey = new SymmetricSecurityKey(key)
				}, out SecurityToken securityToken);
				
				// Decode the authentication type from the claims, only way to bring it with the jwt token :/
				var authenticationType = claimsPrincipal.FindFirst(ClaimTypes.Authentication);
				claimsIdentity = new ClaimsIdentity(claimsPrincipal.Claims, claimsPrincipal.FindFirst( ClaimTypes.Authentication).Value); // well...
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}