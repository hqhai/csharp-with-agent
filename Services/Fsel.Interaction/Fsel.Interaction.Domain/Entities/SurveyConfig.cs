using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using Fsel.Common.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Interaction.Domain.Models.EntityModels;

namespace Fsel.Interaction.Domain.Entities
{
    public class SurveyConfig : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int Tokens { get; set; }

        public string? ApplicableProgramStr { get; set; }

        [NotMapped]
        public IList<EnumSurveyFormType>? ApplicablePrograms
        {
            get { return ConvertHelper.Deserialize<IList<EnumSurveyFormType>>(ApplicableProgramStr); }
            set { ApplicableProgramStr = ConvertHelper.Serialize(value); }
        }

        public string? CompetitionEventIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? CompetitionEventIds
        {
            get { return ConvertHelper.Deserialize<IList<Guid>>(CompetitionEventIdsStr); }
            set { CompetitionEventIdsStr = ConvertHelper.Serialize(value); }
        }

        public string? ApplicableSubjectsStr { get; set; }

        [NotMapped]
        public IList<SurveyConfigApplicableSubjectModel>? ApplicableSubjects
        {
            get { return ConvertHelper.Deserialize<IList<SurveyConfigApplicableSubjectModel>>(ApplicableSubjectsStr); }
            set { ApplicableSubjectsStr = ConvertHelper.Serialize(value); }
        }

        public string? ProgressRequirementsStr { get; set; }

        [NotMapped]
        public IList<SurveyConfigProgressRequirementModel>? ProgressRequirements
        {
            get { return ConvertHelper.Deserialize<IList<SurveyConfigProgressRequirementModel>>(ProgressRequirementsStr); }
            set { ProgressRequirementsStr = ConvertHelper.Serialize(value); }
        }

        public EnumSurveyConfigStatus Status { get; set; }

        public ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();
        public ICollection<UserSurveyAssignment> UserSurveyAssignments { get; set; } = new List<UserSurveyAssignment>();
    }
}
