using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace _2280613193_webdocongnghe.Models
{
    public class MailtrapEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("9797066c516b26", "137b770932def8"),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("no-reply@example.com", "Web Đồ Công Nghệ"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
