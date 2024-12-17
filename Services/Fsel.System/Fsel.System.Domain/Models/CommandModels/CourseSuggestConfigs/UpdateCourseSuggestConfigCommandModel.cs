// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseSuggestConfigs
{
    using global::System;

    public class UpdateCourseSuggestConfigCommandModel : CreateCourseSuggestConfigCommandModel
    {
        public Guid Id { get; set; }
    }
}
