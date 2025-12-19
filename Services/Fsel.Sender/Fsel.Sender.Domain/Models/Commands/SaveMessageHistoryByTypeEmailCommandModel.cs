// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Commands
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;

    public class SaveMessageHistoryByTypeEmailCommandModel
    {
        public string? From { get; set; }

        public IList<string> ToEmails { get; set; } = new List<string>();

        public IList<string> BccEmails { get; set; } = new List<string>();

        public IList<string> CcEmails { get; set; } = new List<string>();

        public string? Content { get; set; }

        public EnumSenderTemplate? Template { get; set; }

        public EnumMessageHistoryStatus Status { get; set; }

        public IList<SendReceiverCommandModel> Receivers { get; set; } = new List<SendReceiverCommandModel>();
    }
}
