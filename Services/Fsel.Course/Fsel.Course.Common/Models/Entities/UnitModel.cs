using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Entities
{
    public class UnitModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}