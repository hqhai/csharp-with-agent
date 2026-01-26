// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.CommandModels
{
    using System;

    public class UpdateStudentLearningContextCommandModel
    {
        public Guid? CourseId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? LevelId { get; set; }
    }
}
