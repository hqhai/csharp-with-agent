// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UserSenderSettingModel
    {
        public Guid SenderConfigId { get; set; }

        public bool IsActive { get; set; }

        public bool IsEdit { get; set; }

        public EnumSenderConfigType Type { get; set; }

        public IList<EnumSenderTemplate>? TemplateEmails { get; set; }
    }
}
