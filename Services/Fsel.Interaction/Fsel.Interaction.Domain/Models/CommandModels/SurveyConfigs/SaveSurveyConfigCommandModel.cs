namespace Fsel.Interaction.Domain.Models.CommandModels.SurveyConfigs
{
    using Fsel.Interaction.Domain.Models.CommandModels.SurveyQuestions;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class SaveSurveyConfigCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Tokens { get; set; }
        public IList<EnumSurveyFormType>? ApplicablePrograms { get; set; }
        public IList<Guid>? CompetitionEventIds { get; set; }
        public IList<SurveyConfigApplicableSubjectModel>? ApplicableSubjects { get; set; }
        public IList<SurveyConfigProgressRequirementModel>? ProgressRequirements { get; set; }
        public IList<CreateSurveyQuestionCommandModel>? SurveyQuestions { get; set; }
    }
}
