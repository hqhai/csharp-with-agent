using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.Unit
{
    public class CreateUnitCommandModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public List<Guid>? LessonIds { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid MockTestId { get; set; }
    }
}