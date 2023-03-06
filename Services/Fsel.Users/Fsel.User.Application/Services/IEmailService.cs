using Fsel.User.Common.ConfigSettings;
using Fsel.User.Common.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.User.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(SendEmailModel message);
    }
    public class EmailService : IEmailService
    {
        private readonly AppSetting _appSetting;

        public EmailService(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task SendEmailAsync(SendEmailModel message)
        {
            try
            {
                var email = new MailMessage();
                email.From = new MailAddress(_appSetting?.Smtp?.From ?? string.Empty);
                email.Subject = message.Subject;
                if (message.To == null) return;
                foreach (var item in message.To)
                {
                    email.To.Add(new MailAddress(item));
                }
                email.Body = message.Content;
                email.IsBodyHtml = true;
                using (var client = new SmtpClient(_appSetting?.Smtp?.SmtpServer))
                {
                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Port = _appSetting?.Smtp?.Port ?? default;
                    client.Credentials = new NetworkCredential(_appSetting?.Smtp?.Username, _appSetting?.Smtp?.Password);
                    client.EnableSsl = true;
                    await client.SendMailAsync(email);
                }
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
