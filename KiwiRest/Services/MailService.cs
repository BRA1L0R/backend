using System;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Policy;
using KiwiRest.Models;

namespace KiwiRest.Services
{
	public static class MailService
	{
		public static bool SendConfirmation(User user, ClaimsIdentity identity)
		{
			using (SmtpClient client = new SmtpClient(Environment.GetEnvironmentVariable("smtphost"), Int32.Parse(Environment.GetEnvironmentVariable("smtpport") ?? "25")))
			{
				try
				{
					var token = Jwt.Sign(identity);

					client.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("smtpfrom"),
						Environment.GetEnvironmentVariable("smtpkey"));
					MailMessage confirmationMessage = new MailMessage(Environment.GetEnvironmentVariable("smtpfrom") ?? throw new Exception("SMTPFROM_EMPTY"),
						user.email,
						"Account Confirmation",
						Flurl.Url.Combine(Environment.GetEnvironmentVariable("apihost"), $"/register/confirm/{token}"));

					client.EnableSsl = true;
					client.Send(confirmationMessage);
					return true;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}
	}
}