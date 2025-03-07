// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class TokenHistoryQueueModel
    {
        public double VolatileToken { get; set; }
        public Guid UserId { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseResultId { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission? Mission { get; set; }
        public EnumTokenHistoryType Type { get; set; }
        public string? EventCode { get; set; }
        public object? Config { get; set; }
        public IList<TokenHistoryTranslationModel>? TokenHistoryTranslations { get; set; }
    }
}
