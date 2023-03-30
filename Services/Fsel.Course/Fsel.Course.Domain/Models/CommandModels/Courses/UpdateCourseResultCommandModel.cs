// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Courses
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.CommandModels.CourseResults;

    public class UpdateCourseResultCommandModel
    {
        public Guid CourseId { get; set; }
        public IList<CreateCourseResultCommandModel>? CourseResults { get; set; }
        public IList<CreateLessonResultCommandModel>? LessonResults { get; set; }
        public IList<CreateUnitResultCommandModel>? UnitResults { get; set; }
    }
}
