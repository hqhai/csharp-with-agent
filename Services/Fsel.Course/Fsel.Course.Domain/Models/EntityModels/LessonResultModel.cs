// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class LessonResultModel : BaseModel
    {
        public double Percent { get; set; }

        public EnumResultStatus Status { get; set; }

        public string? SummaryNote { get; set; }

        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }

        public Guid LessonId { get; set; }

        public Guid StudentId { get; set; }
        public IList<LessonNoteModel>? LessonNotes { get; set; }
    }
}