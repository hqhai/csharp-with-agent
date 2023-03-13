using Fsel.Course.Common.Models.Commands.UnitSkillMockTest;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Commands.Unit
{
    public class CreateUnitCommandModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public List<Guid>? LessonIds { get; set; }

        public List<Guid>? MockTestIds { get; set; }

        public EnumUnitType UnitType { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public List<CreateUnitSkillMockTestCommandModel>? UnitSkillMockTest { get; set; }
    }
}