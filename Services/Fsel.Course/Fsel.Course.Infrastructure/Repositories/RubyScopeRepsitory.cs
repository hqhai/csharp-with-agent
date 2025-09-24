// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;
    using Microsoft.EntityFrameworkCore;

    public class RubyScopeRepsitory : BaseRepository<RubyScope>, IRubyScopeRepository
    {
        public RubyScopeRepsitory(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base
    (dbContext, readDbContext, authContext, mapper)
        {

        }

        public async Task<(bool found, string? nfc)> TryGetBaseTextAsync(string hostType, Guid hostId, string fieldKey, CancellationToken ct)
        {
            var result = await Queryable.FirstOrDefaultAsync(x => x.HostType == hostType && x.HostId == hostId && x.FieldKey == fieldKey, ct);
            if (result == null)
            {
                return (false, null);
            }
            return (true, result.Text);
        }

        public Task<bool> TryUpdateBaseTextAsync(string hostType, Guid hostId, string fieldKey, string baseTextNfc, CancellationToken ct)
            => Task.FromResult(false);
    }
}
