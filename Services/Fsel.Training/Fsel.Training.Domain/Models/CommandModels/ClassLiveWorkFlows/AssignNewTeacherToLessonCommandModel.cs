// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows
{
    using System;

    public class AssignNewTeacherToLessonCommandModel
    {
        public Guid ClassLiveWorkId { get; set; }
        public Guid TeacherId { get; set; }
    }
}
