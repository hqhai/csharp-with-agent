// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.CommandModels
{
    using System;
    using Fsel.Shared.Enums;

    public class CheckSurveyBySurveyFormTypeModel
    {
        public EnumSurveyFormType SurveyFormType { get; set; }

        public Guid? CompetitionEventId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
    }
}
