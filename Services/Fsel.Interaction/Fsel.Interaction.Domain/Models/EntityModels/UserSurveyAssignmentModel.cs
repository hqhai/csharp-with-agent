// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserSurveyAssignmentModel : BaseModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumProgressRequirement? ProgressRequirement { get; set; }
        public bool IsView { get; set; }
        public bool IsSurveyQuestBoard { get; set; }
        public Guid SurveyConfigId { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
