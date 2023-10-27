// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.ValueSettings
{
    using Fsel.Common.ValueSettings;

    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
    }

    public class Smtp
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
