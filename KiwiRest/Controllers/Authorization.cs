using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using KiwiRest.Middlewares;
using KiwiRest.Services;
using KiwiRest.Models;
using Microsoft.AspNetCore.Mvc;
using SHA512 = KiwiRest.Services.SHA512;

namespace KiwiRest.Controllers
{
	[ApiController]
	public class Authorization : ControllerBase
	{
		[HttpPost("/register")]
		public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password, [FromForm] DateTime birthday)
		{
			if (String.IsNullOrEmpty(username) | String.IsNullOrEmpty(email) | String.IsNullOrEmpty(password))
			{
				return BadRequest();
			}

			if (username.Length <= 5) return Problem("Username too short");
			if (password.Length <= 5) return Problem("Password too weak");

			password = SHA512.Hash(password);
			username = StringSanitization.Sanitize(username);
			email = StringSanitization.Sanitize(email);
			if (UserDatabase.GetUser(email, username) != null) return Conflict("Email / username already registered");
			
			int apiTokenLength = Int32.Parse(Environment.GetEnvironmentVariable("apitokenlength") ?? throw new Exception("apitokenlength_ENV_VAR_NULL"));

			User user = new User
			{
				password = password,
				username = username,
				email = email,
				confirmed = false,
				date_of_birth = birthday,
				plan = Plans.Basic,
				registration_timestamp = DateTime.Now,
				role = Roles.User,
				api_token = StringGeneration.RandomString(apiTokenLength) // Generate a random api token which will be used to access the kew value database
			};

			if (!MailService.SendConfirmation(user, user.ClaimsPrincipal(TokenScope.Registration).Identity as ClaimsIdentity)) return Problem("Internal server error", null, 500);
			UserDatabase.RegisterUser(user);
			return Ok();
		}

		[HttpGet("/register/confirm/{token}")]
		public IActionResult ConfirmRegistration(string token)
		{ 
			var tokenValidation = Jwt.Validate(token, out ClaimsIdentity claimsIdentity);
			if (!tokenValidation) return Unauthorized("Token invalid or expired");
			if (claimsIdentity.AuthenticationType != TokenScope.Registration)
				return Unauthorized("JWT was generated for another scope");
			
			var username = claimsIdentity.FindFirst(claim => claim.Type == ClaimTypes.Name).Value;
			if (String.IsNullOrEmpty(username)) return Problem("Invalid username");

			UserDatabase.ConfirmAccount(username);
			return Ok();
		}

		[HttpPost("/login")]
		public IActionResult Login([FromForm] string email, [FromForm] string password)
		{
			email = StringSanitization.Sanitize(email);
			password = SHA512.Hash(password);

			User user = UserDatabase.GetUser(email, null);
			if (HttpContext.User.Identity.IsAuthenticated) return Unauthorized("User already signed in");
			if (user == null) return NotFound("Email not found in database");
			if (user.password != password) return Unauthorized("Incorrect password");
			if (!user.confirmed) return Problem("User has not confirmed his account yet");

			HttpContext.User = user.ClaimsPrincipal(TokenScope.UserLogin);
			return NoContent();

			// return Ok(new
			// {
			// 	token = Jwt.Sign(user, Scope.Authentication), user.username
			// });
		}

		[HttpGet("/api/getApiKey")]
		public IActionResult GetApiKey()
		{
			if (HttpContext.User.Identity.AuthenticationType != TokenScope.UserLogin)
				return Unauthorized();

			return Ok(UserDatabase.GetApiToken(Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value)));
		}
	}
}