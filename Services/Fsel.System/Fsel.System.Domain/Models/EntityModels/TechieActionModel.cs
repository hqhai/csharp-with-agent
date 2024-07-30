// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
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

        public TechieModel? Techie { get; set; }

        public IList<StudentTechieModel>? StudentTechies { get; set; }

        [NotMapped]
        public TechieConfig? Config
        {
            get
            {
                return ConvertHelper.Deserialize<TechieConfig>(ConfigStr);
            }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        //public IList<TechieActionTranslationModel>? Translations { get; set; }
    }

    public class TechieActionTranslationModel : ITranslationObject
    {
        public string? TemplateMessage { get; set; }

        public Guid TechieActionId { get; set; }

        public string? Language { get; set; }
    }
}
