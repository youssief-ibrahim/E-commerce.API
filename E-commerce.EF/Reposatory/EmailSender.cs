using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.IReposatory;

namespace E_commerce.EF.Reposatory
{
    public class EmailSender: IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Your Gmail
            var fromMail = "youssiefibrahim3@gmail.com";

            // Your App Password (NOT Gmail password)
            var fromPassword = "eyhk ucfi zuvl eqgn";

            var message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.To.Add(email);
            message.Subject = subject;
            message.Body = $"<html><body>{htmlMessage}</body></html>";
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(fromMail, fromPassword);

                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
