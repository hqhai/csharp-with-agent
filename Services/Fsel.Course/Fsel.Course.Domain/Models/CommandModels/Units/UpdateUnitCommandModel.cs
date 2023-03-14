using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Units
{
    public class UpdateUnitCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public List<Guid>? LessonIds { get; set; }

        public EnumUnitType UnitType { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid MockTestId { get; set; }

        public EnumMockTestType MockTestType { get; set; }
    }
}
