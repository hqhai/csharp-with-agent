// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;

namespace Fsel.Course.Domain.Models.CommandModels.Lessons
{
    public class UpdateLessonCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? InstructionContent { get; set; }
        public IList<Guid>? HomeWorkIds { get; set; }
        public IList<Guid>? VideoIds { get; set; }
        public Guid ClassForumId { get; set; }
        public IList<Guid>? ExtraPracticeIds { get; set; }
        public IList<UpdateLessonInstructionCommandModel>? LessonInstructions { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
