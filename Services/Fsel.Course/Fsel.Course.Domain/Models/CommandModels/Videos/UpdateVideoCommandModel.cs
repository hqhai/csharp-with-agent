using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Videos
{
    public class UpdateVideoCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
