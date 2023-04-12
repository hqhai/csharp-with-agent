// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.ClassForums;
using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;

namespace Fsel.Course.Domain.Models.CommandModels.Lessons
{
    public class UpdateLessonCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public IList<Guid>? HomeWorkIds { get; set; }
        public IList<Guid>? VideoIds { get; set; }
        public UpdateClassForumCommandModel? ClassForum { get; set; }
        public IList<Guid>? ExtraPracticeIds { get; set; }
        public IList<UpdateLessonInstructionCommandModel>? LessonInstructions { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
