using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWebApi_v1.Service.MailService.Models;

namespace TestWebApi_v1.Service.MailService.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfigration _econfigration;
        public EmailService(EmailConfigration econfigration)
        {
            _econfigration = econfigration;
        }

        public void SendEmail(Message mess)
        {
            var emailMessage = CreateEmailMessage(mess);
            Send(emailMessage);
        }
        private MimeMessage CreateEmailMessage(Message mess)
        {
            var emailMessage=new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _econfigration.From));
            emailMessage.To.AddRange(mess.To);
            emailMessage.Subject = mess.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = mess.Content
            };
            return emailMessage;
        }
        private void Send(MimeMessage emailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_econfigration.StmpServer, _econfigration.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_econfigration.Username, _econfigration.Password);
                client.Send(emailMessage);
            }
            catch
            {
                throw;
            }finally 
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }


    }
}
