using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Courses
{
    public class CreateCourseCommandModel
    {
        public string? Name { get; set; }

        public int NumberOfUnits { get; set; }

        public int NumberOfLessons { get; set; }

        public bool IsPublish { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public IList<CourseUnitMockTestModel>? CourseUnitMockTests { get; set; }
    }
}
