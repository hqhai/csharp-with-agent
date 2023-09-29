// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class LessonResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public EnumResultStatus Status { get; set; }
        public string? SummaryNote { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }

        public Guid LessonId { get; set; }
        public int DisplayOrder { get; set; }
        public Guid StudentId { get; set; }
        public IList<LessonNoteModel>? LessonNotes { get; set; }
    }
}
