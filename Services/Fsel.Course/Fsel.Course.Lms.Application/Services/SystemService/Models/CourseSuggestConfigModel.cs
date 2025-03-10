// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseSuggestConfigModel : BaseModel
    {
        public int FromAge { get; set; }

        public int ToAge { get; set; }

        public EnumPlacementTestLevel PlacementTestLevel { get; set; }

        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}
