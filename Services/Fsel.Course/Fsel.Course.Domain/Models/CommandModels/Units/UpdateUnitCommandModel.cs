using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.UnitSkillMockTests;

namespace Fsel.Course.Domain.Models.CommandModels.Units
{
    public class UpdateUnitCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public List<Guid>? LessonIds { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid MockTestId { get; set; }

        public List<CreateUnitSkillMockTestCommandModel>? UnitSkillMockTest { get; set; }
    }
}
