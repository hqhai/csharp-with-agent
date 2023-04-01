// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Course.Domain.Models.CommandModels.ClassForums;

namespace Fsel.Course.Domain.Models.CommandModels.Lessons
{
    public class CreateLessonCommandModel
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? InstructionContent { get; set; }
        public Guid? TeacherId { get; set; }
        public IList<Guid>? HomeWorkIds { get; set; }
        public IList<Guid>? VideoIds { get; set; }
        public CreateClassForumCommandModel? ClassForum { get; set; }
        public IList<Guid>? ExtraPracticeIds { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
