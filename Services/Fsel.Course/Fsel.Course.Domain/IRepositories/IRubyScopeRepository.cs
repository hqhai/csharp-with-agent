// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;

    public interface IRubyScopeRepository : IRepository<RubyScope>
    {
        Task<(bool found, string? nfc)> TryGetBaseTextAsync(EnumObjectType hostType, Guid hostId, CancellationToken cancellationToken);
        Task<bool> TryUpdateBaseTextAsync(EnumObjectType hostType, Guid hostId, string baseTextNfc, CancellationToken cancellationToken);
    }
}
