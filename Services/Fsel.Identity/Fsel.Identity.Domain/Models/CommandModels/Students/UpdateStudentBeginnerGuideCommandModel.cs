// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using Fsel.Identity.Domain.Entities.BeginnerGuideConfigs;

    public class UpdateStudentBeginnerGuideCommandModel
    {
        public StudentBeginnerGuide? BeginnerGuide { get; set; }
    }
}
