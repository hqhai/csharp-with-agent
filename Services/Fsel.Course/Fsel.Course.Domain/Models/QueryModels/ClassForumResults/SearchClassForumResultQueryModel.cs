// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumResults
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class SearchClassForumResultQueryModel : BaseQueryModel
    {
        public string? LessonName { get; set; }

        public string? UnitName { get; set; }

        public Guid? TeacherId { get; set; }
        public int? LessonDisplayOrder { get; set; }

        public int? UnitDisplayOrder { get; set; }

        [JsonIgnore]
        public EnumClassForumResultStatus? Status { get; set; }
    }
}
