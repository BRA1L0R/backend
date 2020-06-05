using System;
using System.Net;
using System.Net.Mail;
using KiwiRest.Models;

namespace KiwiRest.Services
{
	public abstract class MailService
	{
		public static bool SendConfirmation(Account account)
		{
			using (SmtpClient client = new SmtpClient(Environment.GetEnvironmentVariable("smtphost"), Int32.Parse(Environment.GetEnvironmentVariable("smtpport") ?? "25")))
			{
				try
				{
					var token = Jwt.Sign(account, Scope.Registration);

					client.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("smtpfrom"),
						Environment.GetEnvironmentVariable("smtpkey"));
					MailMessage confirmationMessage = new MailMessage(Environment.GetEnvironmentVariable("smtpfrom"),
						account.Email,
						"Account Confirmation",
						$"{Environment.GetEnvironmentVariable("apihost")}/confirm/{token}");

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