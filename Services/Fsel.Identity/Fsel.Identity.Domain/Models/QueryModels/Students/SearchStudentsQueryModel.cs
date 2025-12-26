namespace Fsel.Identity.Domain.Models.QueryModels.Students
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class SearchStudentsQueryModel : BaseQueryModel
    {
        public Guid? SchoolId { get; set; }
        public Guid? StudentId { get; set; }
        public string? SchoolName { get; set; }
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public bool IsDelete { get; set; }

        [BindNever]
        public IList<string>? Grades
        {
            get
            {
                return ListSchoolGrade.ToList<string>();
            }
        }

        [BindNever]
        public IList<string>? Classes
        {
            get
            {
                return ListSchoolClass.ToList<string>();
            }
        }

        public bool IsCourseProcess { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseType? CourseType { get; set; }
    }
}
