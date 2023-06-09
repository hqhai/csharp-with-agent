// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Commands
{
    using Fsel.Shared.Enums;

    public class SendEmailByTemplateCommandModel : SendEmailCommandModel
    {
        public EnumSenderTemplate? Template { get; set; }
        public object? Params { get; set; }
    }
}
