using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.Videos
{
    public class CreateVideoCommandModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}