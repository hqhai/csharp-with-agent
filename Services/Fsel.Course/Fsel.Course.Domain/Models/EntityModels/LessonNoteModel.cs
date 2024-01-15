// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class LessonNoteModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public Guid LessonResultId { get; set; }

        public EnumNoteType Type { get; set; }
    }
}
