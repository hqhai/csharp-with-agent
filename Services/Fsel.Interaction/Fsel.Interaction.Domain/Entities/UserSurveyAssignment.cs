// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UserSurveyAssignment : Entity
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumProgressRequirement ProgressRequirement { get; set; }
        public bool IsView { get; set; }
        public bool IsDone { get; set; }
        public Guid? SurveyConfigId { get; set; }
    }
}
