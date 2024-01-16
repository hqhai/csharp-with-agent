// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonNotes
{
    using Fsel.Course.Domain.Enums;

    public class CreateLessonNoteCommandModel
    {
        public Guid? LessonResultId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }

        public EnumNoteType Type { get; set; }
    }
}
