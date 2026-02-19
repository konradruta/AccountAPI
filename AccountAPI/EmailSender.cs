
using System.Net;
using System.Net.Mail;

namespace AccountAPI
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmail(string to, string subject, string body)
        {
            var fromAddress = new MailAddress("email@email.pl", "Reset hasła – AccountAPI");
            var toAddress = new MailAddress(to);
            const string fromPassword = "hasloemail";

            var smtp = new SmtpClient
            {
                Host = "smtp.wp.pl",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            await smtp.SendMailAsync(message);
        }
    }
}
