// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonNotes
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateLessonSummaryNoteCommandModel : BaseCommandModel
    {
        public string? SummaryNote { get; set; }
        public Guid LessonResultId { get; set; }
    }
}
