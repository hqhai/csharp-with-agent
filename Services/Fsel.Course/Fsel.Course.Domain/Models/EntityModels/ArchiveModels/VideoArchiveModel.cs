// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class VideoArchiveModel : BaseModel
    {
        public string? Name { get; set; }
        public DateTime? DeletedDate { get; set; }
        public IList<EnumCourseSkill>? Skills { get; set; }
    }
}
