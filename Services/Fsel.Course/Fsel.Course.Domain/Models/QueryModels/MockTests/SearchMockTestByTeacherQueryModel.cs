// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.MockTests
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchMockTestByTeacherQueryModel : BaseQueryModel
    {
        public EnumMockTestFilter? MockTestFilter { get; set; }
        public IList<Guid>? CourseIds { get; set; }
    }
}
