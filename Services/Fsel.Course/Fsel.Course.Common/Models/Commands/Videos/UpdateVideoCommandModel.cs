using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Commands.VideoTimeCode;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.Videos
{
    public class UpdateVideoCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public List<UpdateVideoTimeCodeCommandModel> VideoTimeCodes { get; set; } = new List<UpdateVideoTimeCodeCommandModel>();
    }
}