// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.MockTests
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchMockTestByTeacherQueryModel : BaseQueryModel
    {
        public string? UnitName { get; set; }
        public string? MockTestName { get; set; }
        public EnumMockTestFilter? MockTestFilter { get; set; }
    }
}
