// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class StudentTechieModel : BaseModel
    {
        public string? ConfigStr { get; set; }

        public string? Message { get; set; }

        public TechieAction? TechieAction { get; set; }

        public Guid TechieActionId { get; set; }

        public Guid StudentId { get; set; }

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
