// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Archives
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchItemsInArchiveQueryModel : BaseQueryModel
    {
        public string? ObjectName { get; set; }
        public EnumCourseLevel? Level { get; set; }
        public IList<Guid>? TeacherIds { get; set; }
        public EnumTimeCodeType? TimeCodeType { get; set; }
    }
}
