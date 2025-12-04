// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Ais
{
    using EntityModels.V1i2;

    public class UserAiModel
    {
        public IList<ClassForumAiResponseModel>? ClassForumAIs { get; set; }
        public bool ConditionRetry { get; set; }
    }
}
