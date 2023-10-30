// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;

    public class LessonResultModel : BaseResultScoreModel
    {
        public string? SummaryNote { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public IList<LessonNoteModel>? LessonNotes { get; set; }
    }
}
