// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Entities
{
    public class SmsModel
    {
        public IList<string> PhoneNumber { get; set; } = new List<string>();
        public string? Content { get; set; }
    }
}
