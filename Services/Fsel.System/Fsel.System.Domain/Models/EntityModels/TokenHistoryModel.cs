// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using global::System;

    public class TokenHistoryModel : BaseModel
    {
        public Guid? TokenConfigId { get; set; }
        public double InitialToken { get; set; }
        public double VolatileToken { get; set; }
        public double RemainToken { get; set; }
        public Guid UserId { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseResultId { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission? Mission { get; set; }
        public EnumTokenHistoryType Type { get; set; }
        public object? Config { get; set; }
        public TokenConfigModel? TokenConfig { get; set; }
    }
}
