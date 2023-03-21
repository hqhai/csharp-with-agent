// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Commands
{
    public class SmsCommandModel
    {
        public IList<string> PhoneNumber { get; set; } = new List<string>();
        public string? Content { get; set; }
    }
}
