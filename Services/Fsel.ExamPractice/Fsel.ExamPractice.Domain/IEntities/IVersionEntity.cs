// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.IEntities
{
    using Fsel.ExamPractice.Domain.Enums;

    public interface IVersionEntity
    {
        Guid Id { get; set; }

        Guid OriginalId { get; set; }

        int Version { get; set; }

        EnumVersionStatus VersionStatus { get; set; }
    }
}
