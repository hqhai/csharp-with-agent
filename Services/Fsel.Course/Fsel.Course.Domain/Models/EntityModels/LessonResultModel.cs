// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Enums;

    public class LessonResultModel
    {
        public double Percent { get; set; }

        public EnumResultStatus Status { get; set; }

        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }

        public Guid LessonId { get; set; }

        public Guid StudentId { get; set; }

        public IList<VideoResultModel>? VideoResults { get; set; }

        public IList<LessonNoteModel>? LessonNotes { get; set; }
    }
}
