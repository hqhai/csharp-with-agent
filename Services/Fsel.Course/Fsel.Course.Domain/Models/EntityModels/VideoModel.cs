using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public EnumVideoType Type { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<VideoTimeCodeModel>? VideoTimeCodes { get; set; }
    }
}
