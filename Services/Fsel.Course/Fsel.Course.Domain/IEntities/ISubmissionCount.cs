// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IEntities
{
    using Fsel.Shared.Enums;

    public interface ISubmissionCount
    {
        public EnumSubmissionCount? SubmissionCount { get; set; }
    }
}
