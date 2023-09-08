// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class LessonDashboardModel : BaseModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public double Percent { get; set; }
        public Guid? MockTestId { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public IList<LessonInstructionModel>? LessonInstructions { get; set; }
        public EnumResultStatus StatusClassForum { get; set; }
        public EnumResultStatus StatusVideo { get; set; }
        public EnumResultStatus StatusHomeWork { get; set; }
    }
}
