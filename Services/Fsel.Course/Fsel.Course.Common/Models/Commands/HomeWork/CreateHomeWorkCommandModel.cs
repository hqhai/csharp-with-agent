using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.HomeWork
{
    public class CreateHomeWorkCommandModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public string? MediaPost { get; set; }
        public bool IsActive { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}