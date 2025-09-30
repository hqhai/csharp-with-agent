// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class RubyScopeRepsitory : BaseRepository<RubyScope>, IRubyScopeRepository
    {
        public RubyScopeRepsitory(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base
    (dbContext, readDbContext, authContext, mapper)
        {

        }

        public async Task<(bool found, string? nfc)> TryGetBaseTextAsync(EnumObjectType hostType, Guid hostId, CancellationToken ct)
        {
            var result = await Queryable.FirstOrDefaultAsync(x => x.ObjectType == hostType && x.ObjectId == hostId, ct);
            if (result == null)
            {
                return (false, null);
            }
            return (true, result.Text);
        }

        public Task<bool> TryUpdateBaseTextAsync(EnumObjectType hostType, Guid hostId, string baseTextNfc, CancellationToken ct)
            => Task.FromResult(false);
    }
}
