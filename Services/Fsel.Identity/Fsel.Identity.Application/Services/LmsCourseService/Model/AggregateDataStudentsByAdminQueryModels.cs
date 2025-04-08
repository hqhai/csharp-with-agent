namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class AggregateDataStudentsByAdminQueryModels
    {
        public IList<AggregateDataStudentsByAdminQueryModel>? Students { get; set; }
    }

    public class AggregateDataStudentsByAdminQueryModel
    {
        public Guid? StudentId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
