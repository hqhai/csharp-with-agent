// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class TechieActionModel : BaseModel
    {
        public string? ConfigStr { get; set; }

        public string? TemplateMessage { get; set; }

        public int Priority { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public EnumTechieAction Action { get; set; }
        public EnumTechieFeature Feature { get; set; }

        public Guid TechieId { get; set; }

        public Techie Techie { get; set; } = new Techie();

        public IList<StudentTechie>? StudentTechies { get; set; }

        [NotMapped]
        public TechieConfig? Config
        {
            get
            {
                return ConvertHelper.Deserialize<TechieConfig>(ConfigStr);
            }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }
    }
}
