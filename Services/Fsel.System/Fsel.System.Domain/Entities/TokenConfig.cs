// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class TokenConfig : Entity
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public int DisplayOrder { get; set; }
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<TokenHistory> TokenHistories { get; set; } = new List<TokenHistory>();
    }
}
