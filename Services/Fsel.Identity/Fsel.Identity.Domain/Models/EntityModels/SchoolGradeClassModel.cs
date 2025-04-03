namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class SchoolGradeClassModel
    {
        public bool IsHaveConfigStudent { get; set; }
        public bool IsHaveConfigTeacher { get; set; }
        public bool IsExportAllowed { get; set; }
        public IList<SchoolGradeClass>? GradesAndClasses { get; set; }
    }

    public class SchoolGradeClass
    {
        public string? Grade { get; set; }
        public IList<string>? Classes { get; set; }
    }
}
