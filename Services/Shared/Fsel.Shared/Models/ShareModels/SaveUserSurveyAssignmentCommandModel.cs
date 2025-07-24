// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SaveUserSurveyAssignmentCommandModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumProgressRequirement ProgressRequirement { get; set; }
        public bool IsSurveyQuestBoard { get; set; }
    }
}
