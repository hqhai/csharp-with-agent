using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    public class SurveyConfigModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Tokens { get; set; }
        public int CompletedUserCount { get; set; }
        public IList<EnumSurveyFormType>? ApplicablePrograms { get; set; }
        public IList<Guid>? CompetitionEventIds { get; set; }
        public EnumSurveyConfigStatus Status { get; set; }
        public IList<SurveyConfigApplicableSubjectModel>? ApplicableSubjects { get; set; }
        public IList<SurveyConfigProgressRequirementModel>? ProgressRequirements { get; set; }
        public IList<SurveyGroupQuestionModel>? SurveyGroupQuestions { get; set; }
    }

    public class SurveyGroupQuestionModel
    {
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int DisplayLevel { get; set; }
        public IList<SurveyQuestionModel>? SurveyQuestions { get; set; }
    }
}
