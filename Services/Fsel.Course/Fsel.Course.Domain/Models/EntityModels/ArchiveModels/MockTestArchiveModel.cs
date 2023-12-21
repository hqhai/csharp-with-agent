// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class MockTestArchiveModel : BaseModel
    {
        public string? Name { get; set; }
        public IList<EnumCourseSkill>? Skills { get; set; }
        public EnumMockTestType MockTestType { get; set; }

    }
}
