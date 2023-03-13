using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseModel : BaseEntityModel
    {
        public string? Name { get; set; }
        public int NumberOfUnits { get; set; }

        public int NumberOfLessons { get; set; }

        public bool IsPublish { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public List<CourseUnitMockTest>? CourseUnitMockTests { get; set; }
    }
}
