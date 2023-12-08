using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWebApi_v1.Service.MailService.Models;

namespace TestWebApi_v1.Service.MailService.Service
{
    public interface IEmailService
    {
        void SendEmail(Message mess);
    }
}
