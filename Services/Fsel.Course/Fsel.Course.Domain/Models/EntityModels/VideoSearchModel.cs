// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class VideoSearchModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumVideoType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<ExcerciseSearchModel>? Excercises { get; set; }
        public ICollection<VideoTimeCodeSearchModel>? VideoTimeCodes { get; set; }
    }
}
