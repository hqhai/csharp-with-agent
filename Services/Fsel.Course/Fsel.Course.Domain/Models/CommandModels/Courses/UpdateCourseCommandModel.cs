using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;
using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;

namespace Fsel.Course.Domain.Models.CommandModels.Courses
{
    public class UpdateCourseCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public int NumberOfUnits { get; set; }

        public int NumberOfLessons { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public IList<UpdateCourseUnitMockTestCommandModel>? CourseUnitMockTests { get; set; }
    }
}
