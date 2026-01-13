// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class SenderConfig : Entity
    {
        public EnumSenderConfigType Type { get; set; }

        public string? TemplateEmailStr { get; set; }

        [NotMapped]
        public IList<EnumSenderTemplate>? TemplateEmails
        {
            get
            {
                return ConvertHelper.Deserialize<IList<EnumSenderTemplate>>(TemplateEmailStr);
            }
            set { TemplateEmailStr = ConvertHelper.Serialize(value); }
        }

        public bool IsEdit { get; set; }
    }
}
