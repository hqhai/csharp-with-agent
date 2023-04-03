// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class LessonNoteModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public LessonResult? LessonResult { get; set; }
        public Guid LessonResultId { get; set; }
        public IList<LessonNote>? LessonNotes { get; set; }
    }
}
