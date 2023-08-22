// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExtraPracticeSearchModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public ExtraPracticeResultModel? ExtraPracticeResult { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid? UnitId { get; set; }
        public string? NameUnit { get; set; }
        public long? AccessCount { get; set; }
        public double? Percent { get; set; }
    }
}
