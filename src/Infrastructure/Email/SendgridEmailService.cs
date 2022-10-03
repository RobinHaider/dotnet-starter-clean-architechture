using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Email
{
    public class SendgridEmailService
    {
        private readonly IConfiguration _config;
        public SendgridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string userEmail, string emailSubject, string msg)
        {
            var client = new SendGridClient(_config["SendGrid:Key"]);
            var message = new SendGridMessage
            {
                From = new EmailAddress("changethis@email.com", _config["Sendgrid:User"]),
                Subject = emailSubject,
                PlainTextContent = msg,
                HtmlContent = msg
            };
            message.AddTo(new EmailAddress(userEmail));
            // disabled click tracking
            message.SetClickTracking(false, false);

            await client.SendEmailAsync(message);
        }
    }
}