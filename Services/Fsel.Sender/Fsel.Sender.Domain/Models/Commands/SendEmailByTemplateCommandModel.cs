// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Models.Commands
{
    public class SendEmailByTemplateCommandModel : SendEmailCommandModel
    {
        public object? Params { get; set; }
    }
}
