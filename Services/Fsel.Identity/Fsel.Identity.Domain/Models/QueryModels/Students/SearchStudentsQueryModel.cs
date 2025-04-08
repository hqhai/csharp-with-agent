namespace Fsel.Identity.Domain.Models.QueryModels.Students
{
    using Fsel.Core.Base.BaseModels;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class SearchStudentsQueryModel : BaseQueryModel
    {
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }

        public bool IsDelete { get; set; }

        [BindNever]
        public IList<string>? Grades
        {
            get
            {
                return string.IsNullOrWhiteSpace(ListSchoolGrade)
                ? new List<string>()
                : ListSchoolGrade.Split(',')
                                 .Select(s => s.Trim())
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToList();
            }
        }

        [BindNever]
        public IList<string>? Classes
        {
            get
            {
                return string.IsNullOrWhiteSpace(ListSchoolClass)
                ? new List<string>()
                : ListSchoolClass.Split(',')
                                 .Select(s => s.Trim())
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToList();
            }
        }
    }
}
