using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SCP.Application.Services
{

    public class EmailService
    {
        private readonly EmailServiceOptions _emailServiceOptions;

        public EmailService(IOptions<EmailServiceOptions> emailServiceOptions)
        {
            _emailServiceOptions = emailServiceOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailServiceOptions.SenderName, _emailServiceOptions.SenderEmail));
            emailMessage.To.Add(new MailboxAddress(email, email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailServiceOptions.SmtpServer, _emailServiceOptions.SmtpPort, _emailServiceOptions.UseSsl);
                    await client.AuthenticateAsync(_emailServiceOptions.SmtpUsername, _emailServiceOptions.SmtpPassword);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    // Handle exception appropriately
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }

    public class EmailServiceOptions
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
    }

}
