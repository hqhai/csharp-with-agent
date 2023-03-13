using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;

namespace Fsel.Course.Domain.Models.CommandModels.Videos
{
    public class CreateVideoCommandModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid TeacherId { get; set; } = Guid.Empty;

        public EnumCourseLevel CourseLevel { get; set; }
        public List<CreateVideoTimeCodeCommandModel>? VideoTimeCodes { get; set; } = new List<CreateVideoTimeCodeCommandModel>();
    }
}
