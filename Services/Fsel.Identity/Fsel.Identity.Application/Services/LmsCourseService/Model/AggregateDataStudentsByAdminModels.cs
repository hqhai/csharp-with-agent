namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class AggregateDataStudentsByAdminModels
    {
        public IList<AggregateDataStudentsByAdminModel>? Students { get; set; }
    }

    public class AggregateDataStudentsByAdminModel
    {
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public int? TotalLesson { get; set; }
        public int? TotalLessonDone { get; set; }
        public bool IsLearnStudent { get; set; }
        public string? PTStatus { get; set; }
    }
}
