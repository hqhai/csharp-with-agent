// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonNotes
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateLessonNoteCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
    }
}
