// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SetTimeModuleModel
    {
        public string? Type { get; set; }
        public Guid ObjectId { get; set; }
        public double AccessTime { get; set; }
        public EnumSubmissionCount? SubmissionCount { get; set; }
    }
}
