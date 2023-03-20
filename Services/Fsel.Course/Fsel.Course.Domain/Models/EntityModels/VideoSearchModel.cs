// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;

    public class VideoSearchModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<VideoExerciseSearchModel>? Exercises { get; set; }
    }
}
