using Fsel.Shared.Enums;

namespace Fsel.Training.Application.Services.CourseServices.CommandModels
{
    public class GetCourseForChooseLevelQueryModel
    {
        public Guid StudentId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
