// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class TokenHistory : Entity
    {
        public Guid? TokenConfigId { get; set; }

        public double InitialToken { get; set; }

        public double VolatileToken { get; set; }

        public double RemainToken { get; set; }
        public EnumTokenFeature Feature { get; set; }

        public EnumTokenMission? Mission { get; set; }

        public Guid UserId { get; set; }

        public Guid ObjectId { get; set; }

       public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public EnumTokenHistoryType Type { get; set; }

        public TokenConfig? TokenConfig { get; set; }
    }
}
