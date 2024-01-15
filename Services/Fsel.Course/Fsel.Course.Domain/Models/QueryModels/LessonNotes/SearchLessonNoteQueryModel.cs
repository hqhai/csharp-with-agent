// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.LessonNotes
{
    using Fsel.Core.Base.BaseModels;

    public class SearchLessonNoteQueryModel : BaseQueryModel
    {
        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }
    }
}
