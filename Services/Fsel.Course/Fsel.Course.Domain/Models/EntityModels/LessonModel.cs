// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonModel : BaseModel
    {
        public string? InstructionContent { get; set; }

        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public VideoModel? Video { get; set; }

        public IList<HomeWorkModel>? HomeWorks { get; set; }
        public IList<LessonResultModel>? LessonResults { get; set; }
        public IList<LessonInstructionModel>? LessonInstructions { get; set; }
    }
}
