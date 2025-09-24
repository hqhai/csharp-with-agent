// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface IRubyScopeRepository : IRepository<RubyScope>
    {
        Task<(bool found, string? nfc)> TryGetBaseTextAsync(string hostType, Guid hostId, string fieldKey, CancellationToken cancellationToken);
        Task<bool> TryUpdateBaseTextAsync(string hostType, Guid hostId, string fieldKey, string baseTextNfc, CancellationToken cancellationToken);
    }
}
