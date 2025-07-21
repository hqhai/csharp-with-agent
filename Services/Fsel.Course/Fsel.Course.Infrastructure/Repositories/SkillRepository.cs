// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;

    public class SkillRepository : BaseRepository<Skill>, ISkillRepository
    {
        public SkillRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsDuplicateFieldValueAsync(string? fieldName, string? value)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || value == null)
            {
                return false;
            }

            var filter = $"{fieldName} == @0";
            return await Queryable.Where(filter, value).AnyAsync();
        }

        public async Task<bool> IsDuplicateFieldValueAsync(Guid id, string? fieldName, string? value)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || value == null)
            {
                return false;
            }
            string filter = $"{fieldName} == @0 && Id != @1";
            return await Queryable.Where(filter, value, id).AnyAsync();
        }
    }
}
