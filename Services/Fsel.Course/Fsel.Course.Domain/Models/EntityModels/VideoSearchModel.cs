// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class VideoSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public string? SubFilePath { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public IList<VideoExerciseSearchModel>? Exercises { get; set; }
    }
}
