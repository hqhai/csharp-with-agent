using Fsel.Shared.Enums;

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    public class SurveyConfigProgressRequirementModel
    {
        public EnumCourseType CourseType { get; set; }
        public EnumProgressRequirement ProgressRequirement { get; set; }
    }
}
