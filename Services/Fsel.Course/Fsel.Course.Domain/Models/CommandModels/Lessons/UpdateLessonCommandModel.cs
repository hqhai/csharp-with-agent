using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.CommandModels.Lessons
{
    public class UpdateLessonCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? InstructionContent { get; set; }
        public Guid? TeacherId { get; set; }
        public ICollection<Guid>? HomeWorkIds { get; set; }
        public ICollection<Guid>? VideoIds { get; set; }
        public Guid ClassForumId { get; set; }
        public ICollection<Guid>? ExtraPracticeIds { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
