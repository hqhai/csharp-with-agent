// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class UpdateCourseIdOfStudentsCommandModels
    {
        public IList<UpdateCourseIdOfStudentsCommandModel>? Students { get; set; }
    }

    public class UpdateCourseIdOfStudentsCommandModel
    {
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ClassId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
