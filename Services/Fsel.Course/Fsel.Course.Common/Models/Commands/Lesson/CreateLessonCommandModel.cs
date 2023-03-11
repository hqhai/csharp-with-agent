using Fsel.Course.Common.Models.Commands.ClassForum;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.Lesson
{
    public class CreateLessonCommandModel
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? InstructionContent { get; set; }
        public bool IsActive { get; set; }
        public Guid? TeacherId { get; set; }
        public List<Guid>? HomeWorkIds { get; set; }
        public List<Guid>? VideoIds { get; set; }
        public CreateClassForumCommandModel? ClassForum { get; set; }
        public List<Guid>? ExtraPracticeIds { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}