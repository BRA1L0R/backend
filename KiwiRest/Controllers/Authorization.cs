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

			User user = new User
			{
				password = password,
				username = username,
				email = email,
				confirmed = false,
				date_of_birth = birthday,
				plan = Plans.Basic,
				registration_timestamp = DateTime.Now,
				role = Roles.User
			};

			if (!MailService.SendConfirmation(user, user.ClaimsPrincipal(JwtScope.Registration).Identity as ClaimsIdentity)) return Problem("Internal server error", null, 500);
			UserDatabase.RegisterUser(user);
			return Ok();
		}

		[HttpGet("/register/confirm/{token}")]
		public IActionResult ConfirmRegistration(string token)
		{ 
			var tokenValidation = Jwt.Validate(token, out ClaimsIdentity claimsIdentity);
			if (!tokenValidation) return Unauthorized("Token invalid or expired");
			if (claimsIdentity.AuthenticationType != JwtScope.Registration)
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

			User user = UserDatabase.GetUser(email);
			if (HttpContext.User.Identity.IsAuthenticated) return Unauthorized("User already signed in");
			if (user == null) return NotFound("Email not found in database");
			if (user.password != password) return Unauthorized("Incorrect password");
			if (!user.confirmed) return Problem("User has not confirmed his account yet");

			HttpContext.User = user.ClaimsPrincipal(JwtScope.UserLogin);
			return NoContent();

			// return Ok(new
			// {
			// 	token = Jwt.Sign(user, Scope.Authentication), user.username
			// });
		}

		[HttpGet("/api/getApiKey")]
		public IActionResult GetApiKey()
		{
			if (HttpContext.User.Identity.AuthenticationType != JwtScope.UserLogin)
				return Unauthorized();
			
			ClaimsIdentity identity = new ClaimsIdentity(HttpContext.User.Claims, JwtScope.DatabaseAccess);
			identity.RemoveClaim((from claim in identity.Claims where claim.Type == ClaimTypes.Authentication select claim).Single());	// remove the precedent signing from the claims
			
			return Ok(Jwt.Sign(identity, Int32.MaxValue));
		}
	}
}