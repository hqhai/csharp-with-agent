// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumResults
{
    using Fsel.Core.Base.BaseModels;

    public class SearchClassForumResultQueryModel : BaseQueryModel
    {
        public string? LessonName { get; set; }

        public string? UnitName { get; set; }

        public Guid? TeacherId { get; set; }
        public int? LessonDisplayOrder { get; set; }

        public int? UnitDisplayOrder { get; set; }

        public Guid? CourseId { get; set; }

        public string? CourseCode { get; set; }
    }
}
