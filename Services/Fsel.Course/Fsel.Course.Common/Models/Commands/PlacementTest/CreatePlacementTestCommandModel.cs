using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.PlacementTest
{
    public class CreatePlacementTestCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}