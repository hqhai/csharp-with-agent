// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class CourseSuggestConfigModel : BaseModel
    {
        public int FromAge { get; set; }

        public int ToAge { get; set; }

        public EnumCourseLevel PlacementTestLevel { get; set; }

        public EnumCourseSuggestType Type { get; set; }

        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}
