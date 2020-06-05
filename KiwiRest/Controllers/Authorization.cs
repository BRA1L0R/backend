using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using KiwiRest.Models;
using KiwiRest.Services;
using Microsoft.AspNetCore.Mvc;

namespace KiwiRest.Controllers
{
	[ApiController]
	public class Authorization : ControllerBase
	{
		private string Sanitize(string str) {
			StringBuilder sb = new StringBuilder();
			foreach (char c in str) {
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '@') {
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
		
		[HttpPost("/register")]
		public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
		{
			if (String.IsNullOrEmpty(username) | String.IsNullOrEmpty(email) | String.IsNullOrEmpty(password))
			{
				return BadRequest();
			}

			if (username.Length <= 5) return Problem("Username too short");
			if (password.Length <= 5) return Problem("Password too weak");
			
			SHA512 sha = new SHA512CryptoServiceProvider();
			password = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
			username = Sanitize(username);
			email = Sanitize(email);
			
			Account account = new Account()
			{
				HashedPassword = password,
				Username = username,
				Email = email
			};

			if (MailService.SendConfirmation(account)) return Ok();
			return Problem("Internal server error", null, 500);
		}

		[HttpGet("/confirm/{token}")]
		public IActionResult ConfirmRegistration(string token)
		{
			var tokenValidation = Jwt.Validate(token, out string username, out Scope scope);

			if (!tokenValidation) return Unauthorized("Token invalid or expired");
			if (String.IsNullOrEmpty(username)) return Problem("Invalid username");
			if (scope != Scope.Registration) return Problem("Invalid token scope");
			
			return Ok();
		}
	}
}