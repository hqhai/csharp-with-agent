using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseModel : BaseModel
    {
        public string? Name { get; set; }

        public int NumberOfUnits { get; set; }

        public int NumberOfLessons { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? TeacherId { get; set; }

        public IList<CourseUnitMockTestModel>? CourseUnitMockTests { get; set; }
        public IList<CourseTeacherModel>? CourseTeachers { get; set; }
    }
}
