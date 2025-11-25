// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Commands
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using Microsoft.AspNetCore.Http;

    public class SendEmailCommandModel
    {
        public IList<string> ToEmails { get; set; } = new List<string>();
        public IList<string> BccEmails { get; set; } = new List<string>();
        public IList<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public IList<IFormFile>? Attachments { get; set; }
        public bool? IsCCEmail { get; set; }
        public bool? IsCCEmailDefault { get; set; }
        public EnumSenderTemplate? Template { get; set; }
        public IList<SendReceiverCommandModel> Receivers { get; set; } = new List<SendReceiverCommandModel>();
    }
}
