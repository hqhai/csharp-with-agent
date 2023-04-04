// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;

    public class LessonNoteListModel
    {
        public Guid LessonResultId { get; set; }
        public string? SummaryNote { get; set; }

        public IList<LessonNoteModel>? LessonNotes { get; set; }
    }
}
