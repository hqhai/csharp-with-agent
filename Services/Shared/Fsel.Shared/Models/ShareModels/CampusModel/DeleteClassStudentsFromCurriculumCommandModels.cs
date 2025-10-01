// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using Fsel.Shared.Enums;

    public class DeleteClassStudentsFromCurriculumCommandModels
    {
        public Guid OldCourseId { get; set; }
        public IList<DeleteClassStudentsFromCurriculumCommandModel>? Students { get; set; }
    }

    public class DeleteClassStudentsFromCurriculumCommandModel
    {
        public Guid StudentId { get; set; }
        public Guid? NewCourseId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? CourseCode { get; set; }
        public bool IsUpdateStudent { get; set; }
    }
}
