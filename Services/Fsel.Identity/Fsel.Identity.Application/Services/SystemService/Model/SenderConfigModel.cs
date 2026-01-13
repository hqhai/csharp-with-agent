// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class SenderConfigModel
    {
        public Guid Id { get; set; }

        public EnumSenderConfigType Type { get; set; }

        public IList<EnumSenderTemplate>? TemplateEmails { get; set; }

        public bool IsEdit { get; set; }
    }
}
