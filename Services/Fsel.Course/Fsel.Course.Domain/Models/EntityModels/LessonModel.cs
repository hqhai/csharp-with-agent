using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonModel : BaseModel
    {
        public string? InstructionContent { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public IList<LessonResultModel>? LessonResults { get; set; }
    }
}
