using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWebApi_v1.Service.MailService.Models
{
    public class EmailConfigration
    {
        public string From { get; set; } = null!;
        public string StmpServer { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
