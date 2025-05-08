// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class CourseSuggestConfig : Entity
    {
        [Range(0, 150, ErrorMessage = nameof(EnumCourseSuggestConfigErrorCode.AgeMustBeBetweenZeroAndOneHundredFifty))]
        public int FromAge { get; set; }

        [Range(0, 150, ErrorMessage = nameof(EnumCourseSuggestConfigErrorCode.AgeMustBeBetweenZeroAndOneHundredFifty))]
        public int ToAge { get; set; }

        public EnumCourseLevel PlacementTestLevel { get; set; }

        public EnumCourseSuggestType Type { get; set; }

        public string? CourseLevelStr { get; set; }

        [NotMapped]
        public IList<EnumCourseLevel>? CourseLevels
        {
            get { return ConvertHelper.Deserialize<IList<EnumCourseLevel>>(CourseLevelStr); }
            set { CourseLevelStr = ConvertHelper.Serialize(value); }
        }
    }
}
