// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SectionDetailModel : BaseModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public int DisplayOrder { get; set; }
        public Guid QuestionId { get; set; }
        public Guid SectionTimeCodeId { get; set; }
        public EnumCurrentStatus Status { get; set; }
        public IList<Guid>? QuestionIds { get; set; }
        public IList<SectionTimeCodeModel>? SectionTimeCodes { get; set; }
    }
}
