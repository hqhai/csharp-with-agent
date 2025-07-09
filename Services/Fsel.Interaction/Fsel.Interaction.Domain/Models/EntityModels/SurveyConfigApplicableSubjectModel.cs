namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class SurveyConfigApplicableSubjectModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<EnumSurveyConfigApplicableSubject>? ApplicableSubjects { get; set; }
    }
}
