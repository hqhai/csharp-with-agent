// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SenderConfigModel : BaseModel
    {
        public EnumSenderConfigType Type { get; set; }

        public IList<EnumSenderTemplate>? TemplateEmails { get; set; }

        public bool IsEdit { get; set; }
    }
}
