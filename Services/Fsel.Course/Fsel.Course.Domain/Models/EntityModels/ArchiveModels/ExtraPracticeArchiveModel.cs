// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExtraPracticeArchiveModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
