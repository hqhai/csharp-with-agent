using Fsel.Common.ConfigSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.User.Common.ConfigSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Url? Url { get; set; }
    }

    public class Smtp 
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class Url 
    {
        public string? EmailConfirmUrl { get; set; }
    }
}
