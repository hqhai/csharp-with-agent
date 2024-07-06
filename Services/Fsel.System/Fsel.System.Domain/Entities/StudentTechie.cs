// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Models.ShareModels;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class StudentTechie : Entity
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
