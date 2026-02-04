// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class AddCourseIdForStudentsCampusCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid ClassId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? LevelId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
